using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using ClassicBot.Classes;
using ClassicWrapped;

namespace ClassicBot {
    public class NetworkManager {
        public Bot ClientBot;
        public ClassicWrapped.ClassicWrapped wSock;
        public object WriteLock;
		public NetworkStream BaseStream;
		public TcpClient BaseSock;

        Thread Handler, TimeoutHandler;
        Dictionary<int, Func<IPacket>> Packets;

        public NetworkManager(Bot Core) {
            WriteLock = new object();
            ClientBot = Core;
            Populate();
        }

        /// <summary>
        /// Populates the packet dictionary with all reconized packet types.
        /// </summary>
        void Populate() {
            Packets = new Dictionary<int, Func<IPacket>> {
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

			ClientBot.RaiseDebugMessage("Packets populated.");
        }

        /// <summary>
        /// Sends a client handshake to the minecraft server.
        /// </summary>
		void DoHandshake() {
			var hs = new Handshake();
			hs.ProtocolVersion = 7;
			hs.Name = ClientBot.Username.PadRight(64);
			hs.Motd = ClientBot.MpPass.PadRight(64);

			if (ClientBot.EnableCpe)
				hs.Usertype = 66;
			else
				hs.Usertype = 0;

			hs.Write(this);
		}

        /// <summary>
        /// Sends CPE ExtInfo and ExtEntry packets to the server.
        /// </summary>
        public void SendCPE() {
            var myExtInfo = new ExtInfo();
            myExtInfo.AppName = "ClassicBot";
            myExtInfo.ExtensionCount = (short)ClientBot.ClientSupportedExtensions.Count;
            myExtInfo.Write(this);

            for (int i = 0; i < ClientBot.ClientSupportedExtensions.Count; i++) {
                var myExtEntry = new ExtEntry();
                myExtEntry.ExtName = Enum.GetName(typeof(CPEExtensions), ClientBot.ClientSupportedExtensions[i]);
                myExtEntry.Version = CPEVersionGet(myExtEntry.ExtName);
                myExtEntry.Write(this);
                ClientBot.RaiseDebugMessage("Sent extension.");
            }
        }

        /// <summary>
        /// Retreives the supported version for a given CPE Extension.
        /// </summary>
        /// <param name="ExtName"></param>
        /// <returns>Supported Extension Version. 0 indiciates no support.</returns>
        public int CPEVersionGet(string ExtName) {
            switch (ExtName) {
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
				var AR = BaseSock.BeginConnect(ClientBot.Ip, ClientBot.Port, null, null);

				using (var wh = AR.AsyncWaitHandle) {
					if (!AR.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false)) {
						BaseSock.Close();
						ClientBot.RaiseErrorMessage("Failed to connect: Timeout.");
						return;
					}

					BaseSock.EndConnect(AR);
				}
			} catch (Exception e) {
				ClientBot.RaiseErrorMessage("Failed to connect: " + e.Message);
				return;
			}

			ClientBot.RaiseInfoMessage("Connected to server.");

			BaseStream = BaseSock.GetStream();
			wSock = new ClassicWrapped.ClassicWrapped();
			wSock._Stream = BaseStream;

			DoHandshake();

			Handler = new Thread(Handle);
			Handler.Start();

            TimeoutHandler = new Thread(Timeout);
            TimeoutHandler.Start();
		}

        /// <summary>
        /// Disconnects the client and stops packet handlers and timeout watchers.
        /// </summary>
		public void Disconnect() {
            if (Handler != null)
                Handler.Abort();

            if (TimeoutHandler != null)
                TimeoutHandler.Abort();

            BaseStream.Close();
            BaseSock.Close();
		}

        /// <summary>
        /// Handles incoming packets.
        /// </summary>
		public void Handle() {
			try {
				byte PacketID = 255;

				while ((PacketID = wSock.ReadByte()) != 255) {
					if (BaseSock.Connected) {
						if (!Packets.ContainsKey((int)PacketID)) {
							ClientBot.RaiseErrorMessage("Received unknown packet! ID: " + PacketID.ToString());
							Disconnect();
						}

						var Packet = Packets[PacketID]();
						Packet.Read(this);
						Packet.Handle(this, ClientBot);

						ClientBot.RaisePacketReceived("ID: " + PacketID.ToString());
					} else {
						Disconnect();
						break;
					}
				}
			} catch (Exception e) {
				if (e.GetType() != typeof(ThreadAbortException)) {
					ClientBot.RaiseErrorMessage("Critical error in handling packets.\n" + e.Message);
					//ClientBot.RaiseErrorMessage(e.Message);
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
