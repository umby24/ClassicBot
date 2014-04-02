using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public struct Vector3s {
        public short X;
        public short Y;
        public short Z;
    }

    public struct Vector3f {
        public float X;
        public float Y;
        public float Z;
    }
        
    #endregion
    /// <summary>
    /// Main container class that provides basic functions and events having to deal with clients.
    /// </summary>
    public class Main {
        #region Variables
		public NetworkManager NM;
        #region Server
        public string Name, MOTD, IP;
        public int Port;
        public DateTime LastActive;
        public WorldContainer ClientWorld;
        public List<Entity> Entities;
        #region CPE
        public bool SupportsCPE = false;
        public int ServerCBLevel;
        public string ServerAppName;
        public short Extensions, ReceivedExtensions;
        public Dictionary<string, int> ServerExtensions;
        #endregion
        #endregion
        public string Username, Password, MPPass;
        public Vector3s Location;
        public byte[] Position;
		public bool EnableCPE, VerifyNames;
		public ServiceTypes Service;
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
        /// <param name="LoginService"></param>
        /// <param name="CPE"></param>
        /// <param name="UN"></param>
        /// <param name="PW"></param>
        /// <param name="VerifyName"></param>
        public Main (ServiceTypes LoginService, bool CPE, string UN="Bot", string PW="Bot", bool VerifyName=true) {
			Username = UN;
			Password = PW;
			Service = LoginService;
			EnableCPE = CPE;
            Location = new Vector3s();
            Position = new byte[2];
            VerifyNames = VerifyName;
		}

        /// <summary>
        /// Connects to a minecraft server. 
        /// </summary>
        /// <param name="ServerNameOrUrl">Can be the name of the server, the full server play url, or left blank.</param>
        /// <param name="IsUrl">Set this to true if 'ServerNameorUrl' is a play url.</param>
        /// <param name="_IP">Ip of the server to connect to. Use this if verify names is off.</param>
        /// <param name="_Port">Port of the server to connect to. Use this if verify names is off.</param>
		public void Connect(string ServerNameOrUrl, bool IsUrl, string _IP="", int _Port=0) {
			if (VerifyNames) {
				var LS = new LoginSystem(this);

				if (!LS.VerifyNames(ServerNameOrUrl, IsUrl)) {
					RaiseErrorMessage("Failed to verify name.");
					return;
				}
			} else {
				IP = _IP;
				Port = _Port;
                MPPass = "Bot";
			}

            Entities = new List<Entity>();
            ClientWorld = new WorldContainer();

			NM = new NetworkManager(this);
			NM.Connect();
		}

        /// <summary>
        /// Disconnects from the minecraft server (if connected) and resets the client's variables.
        /// </summary>
		public void Disconnect() {
			NM.Disconnect();

            Location = new Vector3s();
            Position = new byte[2];

            Entities.Clear();
            ClientWorld = null;
		}

        #region Basic Function
        public void SendChat(string Message) {
            var myChat = new Classes.Message();
            myChat.PlayerID = 1;
            myChat.Text = Message.PadRight(64);

            myChat.Write(NM);
        }

        public void RefreshLocation() {
            var MyLoc = new Classes.PlayerTeleport();
            MyLoc.PlayerID = -1;
            MyLoc.X = Location.X;
            MyLoc.Y = Location.Y;
            MyLoc.Z = Location.Z;
            MyLoc.pitch = Position[0];
            MyLoc.yaw = Position[1];
            MyLoc.Write(NM);
        }
        #endregion
        #region Events
        public void RaiseDebugMessage(string Message) {
			if (DebugMessage != null)
				DebugMessage(Message);
		}
		public void RaiseErrorMessage(string Message) {
            if (ErrorMessage != null)
                ErrorMessage(Message);
		}
		public void RaiseInfoMessage(string Message) {
			if (InfoMessage != null)
				InfoMessage(Message);
		}
		public void RaiseChatMessage(string Message) {
			if (ChatMessage != null)
				ChatMessage(Message);
		}
		public void RaisePacketReceived(string Message) {
			if (PacketReceived != null)
				PacketReceived(Message);
		}
        public void RaiseDisconnected(string Message) {
            if (Disconnected != null)
                Disconnected(Message);
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
        public void RaiseLevelProgress(byte Progress) {
            if (LevelProgressChanged != null)
                LevelProgressChanged(Progress);
        }
        public void RaiseAuthChange(byte NewAuth) {
            if (AuthLevelChanged != null)
                AuthLevelChanged(NewAuth);
        }
        public void RaiseLevelComplete(short X, short Y, short Z) {
            if (LevelComplete != null)
                LevelComplete(X, Y, Z);
        }
        public void RaiseBlockChange(short X, short Y, short Z) {
            if (BlockChanged != null)
                BlockChanged(X, Y, Z);
        }
        public void RaisePlayerMoved(Entity Moved) {
            if (PlayerMoved != null)
                PlayerMoved(Moved);
        }
        public void RaisePlayerJoin(Entity NewEntity) {
            if (PlayerJoined != null)
                PlayerJoined(NewEntity);
        }
        public void RaisePlayerLeft(Entity OldEntity) {
            if (PlayerLeft != null)
                PlayerLeft(OldEntity);
        }
        public void RaiseYouMoved() {
            if (YouMoved != null)
                YouMoved();
        }
        public void RaiseClickDistanceSet(short Value) {
            if (ClickDistanceSet != null)
                ClickDistanceSet(Value);
        }
        public void RaiseHeldBlockChange(byte Block, byte CanChange) {
            if (HeldBlockChanged != null)
                HeldBlockChanged(Block, CanChange);
        }
        public void RaiseHotkeyAdded() {
            if (HotkeyAdded != null)
                HotkeyAdded();
        }
        #endregion
        #endregion
        #region Event Delegates
        public delegate void MessageEventArgs(string Message);
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

        public delegate void ProgressEventArgs(byte Progress);
        public event ProgressEventArgs LevelProgressChanged;
        public event ProgressEventArgs AuthLevelChanged;
        
        public delegate void LocationEventArgs(short X, short Y, short Z);
        public event LocationEventArgs LevelComplete;
        public event LocationEventArgs BlockChanged;
        

        public delegate void EntityEventArgs(Entity NewEntity);
        public event EntityEventArgs PlayerJoined;
        public event EntityEventArgs PlayerLeft;
        public event EntityEventArgs PlayerMoved;

        public delegate void CPEShortEventArgs(short Value);
        public event CPEShortEventArgs ClickDistanceSet;

        public delegate void HeldBlockEventArgs(byte HeldBlock, byte PreventChange);
        public event HeldBlockEventArgs HeldBlockChanged;
		#endregion
    }
}
