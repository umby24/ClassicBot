using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;

using ClassicBot.Classes;

namespace ClassicBot {
    public class NetworkManager {
        public Bot ClientBot;
        public ClassicWrapped.ClassicWrapped WSock;
        public object WriteLock;
		public NetworkStream BaseStream;
		public TcpClient BaseSock;

        Thread _handler, _timeoutHandler;
        Dictionary<int, Func<IPacket>> _packets;

        public NetworkManager(Bot core) {
            WriteLock = new object();
            ClientBot = core;
            Populate();
        }

        /// <summary>
        /// Populates the packet dictionary with all reconized packet types.
        /// </summary>
        void Populate() {
            _packets = new Dictionary<int, Func<IPacket>> {
                {0, () => new Handshake()},
                {1, () => new Ping()},
                {2, () => new LevelInit()},
                {3, () => new LevelChunk()},
                {4, () => new LevelFinalize()},
                {6, () => new SetBlockServer()},
                {7, () => new SpawnPlayer()},
                {8, () => new PlayerTeleport()},
                {9, () => new PosAndOrient()},
                {10, () => new PositionUpdate()},
                {11, () => new OrientationUpdate()},
                {12, () => new DespawnPlayer()},
                {13, () => new Message()},
                {14, () => new Disconnect()},
                {15, () => new UpdateRank()},
                {16, () => new ExtInfo()},
                {17, () => new ExtEntry()},
                {18, () => new SetClickDistance()},
                {19, () => new CustomBlockSupportLevel()},
                {20, () => new HoldThis()},
                {21, () => new SetTextHotKey()},
                {22, () => new ExtAddPlayerName()},
                {23, () => new ExtAddEntity()},
                {24, () => new ExtRemovePlayerName()},
                {25, () => new EnvSetColor()},
                {26, () => new MakeSelection()},
				{27, () => new RemoveSelection()},
				{28, () => new SetBlockPermissions()},
				{29, () => new ChangeModel()},
				{30, () => new EnvSetMapAppearance()},
				{31, () => new EnvSetWeatherType()},
				{32, () => new HackControl()}
            };

			ClientBot.raiseDebugMessage("Packets populated.");
        }

        /// <summary>
        /// Sends a client handshake to the minecraft server.
        /// </summary>
		void DoHandshake() {
			var hs = new Handshake {
			    ProtocolVersion = 7,
			    Name = ClientBot.Username.PadRight(64),
			    Motd = ClientBot.MpPass.PadRight(64),
			    Usertype = (byte) (ClientBot.EnableCpe ? 66 : 0)
			};

            hs.Write(this);
		}

        /// <summary>
        /// Sends CPE ExtInfo and ExtEntry packets to the server.
        /// </summary>
        public void SendCPE() {
            var myExtInfo = new ExtInfo {
                AppName = "ClassicBot",
                ExtensionCount = (short) ClientBot.ClientSupportedExtensions.Count
            };
            myExtInfo.Write(this);

            foreach (var t in ClientBot.ClientSupportedExtensions) {
                var myExtEntry = new ExtEntry {ExtName = Enum.GetName(typeof (CPEExtensions), t)};
                myExtEntry.Version = CPEVersionGet(myExtEntry.ExtName);
                myExtEntry.Write(this);
                ClientBot.raiseDebugMessage("Sent extension.");
            }
        }

        /// <summary>
        /// Retreives the supported version for a given CPE Extension.
        /// </summary>
        /// <param name="extName"></param>
        /// <returns>Supported Extension Version. 0 indiciates no support.</returns>
        public int CPEVersionGet(string extName) {
            switch (extName) {
                case "ClickDistance":
                    return Bot.ClickDistanceVersion;
                case "CustomBlocks":
                    return Bot.CustomBlocksVersion;
                case "HeldBlock":
                    return Bot.HeldBlockVersion;
                case "EmoteFix":
                    return Bot.EmoteFixVersion;
                case "TextHotKey":
                    return Bot.TextHotKetVersion;
                case "ExtPlayerList":
                    return Bot.ExtPlayerListVersion;
                case "EnvColors":
                    return Bot.EnvColorsVersion;
                case "SelectionCuboid":
                    return Bot.SelectionCuboidVersion;
                case "BlockPermissions":
                    return Bot.BlockPermissionsVersion;
                case "ChangeModel":
                    return Bot.ChangeModelVersion;
                case "EnvMapAppearance":
                    return Bot.EnvMapAppearanceVersion;
                case "EnvWeatherType":
                    return Bot.EnvWeatherTypeVersion;
                case "HackControl":
                    return Bot.HackControlVersion;
                case "MessageTypes":
                    return Bot.MessageTypesVersion;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Begins the connection process to the server, including the sending of a handshake once connected.
        /// </summary>
		public void Connect() {
			try {
				BaseSock = new TcpClient();
				var ar = BaseSock.BeginConnect(ClientBot.Ip, ClientBot.Port, null, null);

				using (ar.AsyncWaitHandle) {
				    if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false)) {
				        BaseSock.Close();
				        ClientBot.RaiseErrorMessage("Failed to connect: Timeout.");
				        return;
				    }

				    BaseSock.EndConnect(ar);
				}
			} catch (Exception e) {
				ClientBot.RaiseErrorMessage("Failed to connect: " + e.Message);
				return;
			}

			ClientBot.RaiseInfoMessage("Connected to server.");

			BaseStream = BaseSock.GetStream();
			WSock = new ClassicWrapped.ClassicWrapped {_Stream = BaseStream};

            DoHandshake();

			_handler = new Thread(Handle);
			_handler.Start();

            _timeoutHandler = new Thread(Timeout);
            _timeoutHandler.Start();
		}

        /// <summary>
        /// Disconnects the client and stops packet handlers and timeout watchers.
        /// </summary>
		public void Disconnect() {
            if (_handler != null)
                _handler.Abort();

            if (_timeoutHandler != null)
                _timeoutHandler.Abort();

            BaseStream.Close();
            BaseSock.Close();
		}

        /// <summary>
        /// Handles incoming packets.
        /// </summary>
		public void Handle() {
			try {
				byte packetId;

				while ((packetId = WSock.ReadByte()) != 255) {
					if (BaseSock.Connected) {
						if (!_packets.ContainsKey(packetId)) {
							ClientBot.RaiseErrorMessage("Received unknown packet! ID: " + packetId);
							Disconnect();
						}

						var packet = _packets[packetId]();
						packet.Read(this);
						packet.Handle(this, ClientBot);

						ClientBot.RaisePacketReceived("ID: " + packetId);
					} else {
						Disconnect();
						break;
					}
				}
			} catch (Exception e) {
				if (e.GetType() != typeof(ThreadAbortException)) {
					ClientBot.RaiseErrorMessage("Critical error in handling packets.\n" + e.Message);
					ClientBot.RaiseErrorMessage(e.StackTrace);
				}
			}
		}

        /// <summary>
        /// Watches to ensure that the client has not timed out.
        /// </summary>
        void Timeout() {
            try {
                while (BaseSock.Connected) {
                    if ((ClientBot.LastActive - DateTime.UtcNow).Seconds > 65) {
                        ClientBot.RaiseErrorMessage("No message received for 65 seconds, Timed out.");
                        Disconnect();
                    }
                    Thread.Sleep(1000);
                }
            } catch (Exception e) {
                if (e.GetType() != typeof(ThreadAbortException)) 
                    ClientBot.RaiseErrorMessage(e.Message);
            }
        }
    }
}
