using System;
using System.Collections.Generic;
using ClassicBot.Classes;
using ClassicBot.World;

/*
 * Minecraft Classic Bot library 
 * by Umby24
 * 
 * Can be used for any application, including creating fully-fledged clients.
 * The included use will be for protocol / server debugging, chat, and build functions.
 * 
 * Enjoy, and follow the licence.
 * 
 */

namespace ClassicBot {
    #region Vectors
    public struct Vector3S {
        public short X;
        public short Y;
        public short Z;
    }

    public struct Vector3F {
        public float X;
        public float Y;
        public float Z;
    }
        
    #endregion
    /// <summary>
    /// Main container class that provides basic functions and events having to deal with clients.
    /// </summary>
    public class Bot {
        #region Variables
		public NetworkManager Nm;
        #region Server
        public string Name, Motd, Ip;
        public int Port;
        public DateTime LastActive;
        public WorldContainer ClientWorld;
        public Dictionary<sbyte, Entity> Entities;
        public Dictionary<string, ExtPlayerListEntry> ExtPlayerList;
        public Dictionary<short, ExtPlayerListEntry> NumberPlayerList; 
        #region CPE
        public bool SupportsCpe = false;
        public int ServerCbLevel;
        public string ServerAppName;
        public short Extensions, ReceivedExtensions;
        public Dictionary<string, int> ServerExtensions;
        #endregion
        #endregion
        public string Username, Password, MpPass = "";
        public Vector3S Location;
        public byte[] Position;
		public bool EnableCpe, VerifyNames;
        #region CPE
        public short ClickDistance = 160;
        public byte HeldBlock;
        public bool CanChangeBlock;
        public string AppName = "ClassicBot";
        public List<CPEExtensions> ClientSupportedExtensions;
        public List<TextHotKeyEntry> Hotkeys;

        // -- Extension Versions
        public const int ClickDistanceVersion = 1;
        public const int CustomBlocksVersion = 1;
        public const int CustomBlockSuportlevel = 1;
        public const int HeldBlockVersion = 1;
        public const int EmoteFixVersion = 1;
        public const int TextHotKetVersion = 1;
        public const int ExtPlayerListVersion = 1;
        public const int EnvColorsVersion = 1;
        public const int SelectionCuboidVersion = 1;
        public const int BlockPermissionsVersion = 1;
        public const int ChangeModelVersion = 1;
        public const int EnvMapAppearanceVersion = 1;
        public const int EnvWeatherTypeVersion = 1;
        public const int HackControlVersion = 1;
        public const int MessageTypesVersion = 1;
        #endregion
        #endregion

        /// <summary>
        /// Creates a new MinecraftClient, and initilizes all the required variables.
        /// </summary>
        /// <param name="cpe"></param>
        /// <param name="un"></param>
        /// <param name="pw"></param>
        /// <param name="verifyName"></param>
        public Bot (bool cpe, string un="Bot", string pw="Bot", bool verifyName=true) {
			Username = un;
			Password = pw;
			EnableCpe = cpe;
            Location = new Vector3S();
            Position = new byte[2];
            ClientSupportedExtensions = new List<CPEExtensions>();
            ExtPlayerList = new Dictionary<string, ExtPlayerListEntry>(StringComparer.InvariantCultureIgnoreCase);
            NumberPlayerList = new Dictionary<short, ExtPlayerListEntry>();
            VerifyNames = verifyName;
		}

        /// <summary>
        /// Connects to a minecraft server. 
        /// </summary>
        /// <param name="serverNameOrUrl">Can be the name of the server, the full server play url, or left blank.</param>
        /// <param name="isUrl">Set this to true if 'ServerNameorUrl' is a play url.</param>
        /// <param name="ip">Ip of the server to connect to. Use this if verify names is off.</param>
        /// <param name="port">Port of the server to connect to. Use this if verify names is off.</param>
		public void Connect(string serverNameOrUrl, bool isUrl, string ip="", int port=0) {
			if (VerifyNames) {
                var ls = new ClassicubeInteraction(Username, Password);

                if (!ls.Login()) {
                    RaiseErrorMessage("Failed to verify name.");
                    return;
                }

			    var myServer = isUrl ? ls.GetServerByUrl(serverNameOrUrl) : ls.GetServerInfo(serverNameOrUrl);

			    Ip = myServer.Ip;
			    Port = myServer.Port;
			    MpPass = myServer.Mppass;
			} else {
				Ip = ip;
				Port = port;
			}

            Entities = new Dictionary<sbyte, Entity>();
            ClientWorld = new WorldContainer();

			Nm = new NetworkManager(this);
			Nm.Connect();
		}

        /// <summary>
        /// Disconnects from the minecraft server (if connected) and resets the client's variables.
        /// </summary>
		public void Disconnect() {
			Nm.Disconnect();

            Location = new Vector3S();
            Position = new byte[2];

            Entities.Clear();
            ClientWorld = null;
		}

        #region Basic Function
        public void SendChat(string message) {
            var myChat = new Message {PlayerId = 1, Text = message.PadRight(64)};
            myChat.Write(Nm);
        }

        public void RefreshLocation() {
            var myLoc = new PlayerTeleport {
                PlayerId = -1,
                X = Location.X,
                Y = Location.Y,
                Z = Location.Z,
                Yaw = Position[0],
                Pitch = Position[1]
            };

            myLoc.Write(Nm);
        }

        public void PlaceBlock(int x, int y, int z, byte type) {
            var blockPlace = new SetBlock {
                Block = type,
                X = (short) x,
                Y = (short) y,
                Z = (short) z,
                Mode = Convert.ToByte((type > 0))
            };
            blockPlace.Write(Nm);
        }
        #endregion
        #region Events
        public void RaiseDebugMessage(string message) {
			if (DebugMessage != null)
				DebugMessage(message);
		}
		public void RaiseErrorMessage(string message) {
            if (ErrorMessage != null)
                ErrorMessage(message);
		}
		public void RaiseInfoMessage(string message) {
			if (InfoMessage != null)
				InfoMessage(message);
		}
		public void RaiseChatMessage(string message) {
			if (ChatMessage != null)
				ChatMessage(message);
		}
		public void RaisePacketReceived(string message) {
			if (PacketReceived != null)
				PacketReceived(message);
		}
        public void RaiseDisconnected(string message) {
            if (Disconnected != null)
                Disconnected(message);
        }
        
        #region Non-Message events
        public void RaisePingReceived() {
            if (PingReceived != null)
                PingReceived();
        }
        public void RaiseLevelInit() {
            if (LevelInitiated != null) {
                LevelInitiated();
            }
        }
        public void RaiseLevelProgress(byte progress) {
            if (LevelProgressChanged != null)
                LevelProgressChanged(progress);
        }
        public void RaiseAuthChange(byte newAuth) {
            if (AuthLevelChanged != null)
                AuthLevelChanged(newAuth);
        }
        public void RaiseLevelComplete(short x, short y, short z) {
            if (LevelComplete != null)
                LevelComplete(x, y, z);
        }
        public void RaiseBlockChange(short x, short y, short z) {
            if (BlockChanged != null)
                BlockChanged(x, y, z);
        }
        public void RaisePlayerMoved(Entity moved) {
            if (PlayerMoved != null)
                PlayerMoved(moved);
        }
        public void RaisePlayerJoin(Entity newEntity) {
            if (PlayerJoined != null)
                PlayerJoined(newEntity);
        }
        public void RaisePlayerLeft(Entity oldEntity) {
            if (PlayerLeft != null)
                PlayerLeft(oldEntity);
        }
        public void RaiseYouMoved() {
            if (YouMoved != null)
                YouMoved();
        }
        public void RaiseClickDistanceSet(short value) {
            if (ClickDistanceSet != null)
                ClickDistanceSet(value);
        }
        public void RaiseHeldBlockChange(byte block, byte canChange) {
            if (HeldBlockChanged != null)
                HeldBlockChanged(block, canChange);
        }
        public void RaiseHotkeyAdded() {
            if (HotkeyAdded != null)
                HotkeyAdded();
        }

        public void RaiseExtPlayerListUpdate() {
            if (ExtPlayerListUpdate != null)
                ExtPlayerListUpdate();
        }
        #endregion
        #endregion
        #region Event Delegates
        public delegate void MessageEventArgs(string message);
        /// <summary>
        /// Occurs when an error is thrown by the client.
        /// </summary>
        public event MessageEventArgs ErrorMessage;
        /// <summary>
        /// Provides debug information for Server Developers.
        /// </summary>
		public event MessageEventArgs DebugMessage;
        /// <summary>
        /// Provides basic information to users.
        /// </summary>
		public event MessageEventArgs InfoMessage;
        /// <summary>
        /// Occurs when a chat message is received.
        /// </summary>
		public event MessageEventArgs ChatMessage;
        /// <summary>
        /// Occurs when a packet is received from the server.
        /// </summary>
		public event MessageEventArgs PacketReceived;
        /// <summary>
        /// Occurs when the client is kicked / disconnected from the server.
        /// </summary>
        public event MessageEventArgs Disconnected;

        public delegate void BlankEventArgs();
        public event BlankEventArgs PingReceived;
        public event BlankEventArgs LevelInitiated;
        public event BlankEventArgs YouMoved;
        public event BlankEventArgs HotkeyAdded;
        public event BlankEventArgs ExtPlayerListUpdate;

        public delegate void ProgressEventArgs(byte progress);
        public event ProgressEventArgs LevelProgressChanged;
        public event ProgressEventArgs AuthLevelChanged;
        
        public delegate void LocationEventArgs(short x, short y, short z);
        public event LocationEventArgs LevelComplete;
        public event LocationEventArgs BlockChanged;
        

        public delegate void EntityEventArgs(Entity newEntity);
        public event EntityEventArgs PlayerJoined;
        public event EntityEventArgs PlayerLeft;
        public event EntityEventArgs PlayerMoved;

        public delegate void CpeShortEventArgs(short value);
        public event CpeShortEventArgs ClickDistanceSet;

        public delegate void HeldBlockEventArgs(byte heldBlock, byte preventChange);
        public event HeldBlockEventArgs HeldBlockChanged;
		#endregion
    }
}
