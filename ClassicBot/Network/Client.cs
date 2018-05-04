using System;
using System.Collections.Generic;
using ClassicBot.Common;
using ClassicBot.World;
using Sockets;
using Sockets.EventArgs;

namespace ClassicBot.Network {
    public class Client {
        public readonly ByteBuffer SendBuffer;
        public bool IsCpe;
        public Dictionary<string, int> Extensions;
        public string ServerAppName { get; set; }
        public int ServerExtensionCount { get; set; }
        public Player ClientPlayer;
        public bool SentCpe;

        public readonly ClassicBot Bot;
        private readonly ClientSocket _socket;
        private readonly ByteBuffer _receiveBuffer;
        private Dictionary<byte, IPacket> _packets;
        private DateTime _lastActive;
        private readonly object _handleLock = new object();

        public Client(ClientSocket socket, ClassicBot bot) {
            _socket = socket; // -- Asssign incoming variables
            Bot = bot;
            _receiveBuffer = new ByteBuffer(); // -- Setup send/receive buffers and initiate collections
            SendBuffer = new ByteBuffer();
            Extensions = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            ClientPlayer = new Player(Bot.Username, this, Bot.Mppass); // -- Create a new player container for ourselves
            _lastActive = DateTime.UtcNow;

            // -- Setup event handlers for the socket
            _socket.Connected += SocketOnConnected;
            _socket.DataReceived += SocketOnDataReceived;
            _socket.Disconnected += SocketOnDisconnected;
            SendBuffer.DataAdded += SendBufferOnDataAdded;

            PopulatePackets(); // -- Ready the packets for handling
        }

        public void Connect() {
            _socket.Connect(); // -- Connect to the server
        }

        /// <summary>
        /// Sends a packet to the connected server.
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(IPacket packet) {
            lock (SendBuffer) { // -- make sure something else isn't writing to the send buffer at this time
                packet.Write(SendBuffer);
            }
        }

        /// <summary>
        /// Determines if the client supports the given extension
        /// </summary>
        /// <param name="ext">The name of the extension</param>
        /// <param name="version">The support level of the extension required</param>
        /// <returns>true if supported, false if not.</returns>
        public bool SupportsExtension(string ext, int version) {
            if (!IsCpe)
                return false;

            int extVersion;

            if (Extensions.TryGetValue(ext, out extVersion))
                return version == extVersion;

            return false;
        }

        /// <summary>
        /// Disconnect the client and performs any required cleanup tasks.
        /// </summary>
        public void Shutdown() {
            _socket.Disconnect("Shutting down");
        }

        #region Socket Events
        private void SocketOnConnected(SocketConnectedArgs args) {
            Bot.OnInfoMessage("Connected to server");
            ClientPlayer.SendHandshake();
        }

        private void SendBufferOnDataAdded() {
            _socket.Send(SendBuffer.GetAllBytes());
        }

        private void SocketOnDisconnected(SocketDisconnectedArgs args) {
            Bot.OnInfoMessage("Disconnected from server");
        }

        private void SocketOnDataReceived(DataReceivedArgs args) {
            lock (_handleLock) {
                _receiveBuffer.AddBytes(args.Data);
                _lastActive = DateTime.UtcNow;
                Handle();
            }
        }
        #endregion

        private void PopulatePackets() {
            _packets = new Dictionary<byte, IPacket> {
                {0, new Handshake() },
                {1, new Ping() },
                {2, new LevelInit() },
                {3, new LevelChunk() },
                {4, new LevelFinalize() },
                {6, new SetBlockServer() },
                {7, new SpawnPlayer() },
                {8, new PlayerTeleport() },
                {9, new PosAndOrient() },
                {10, new PositionUpdate() },
                {11, new OrientationUpdate() },
                {12, new DespawnPlayer() },
                {13, new Message() },
                {14, new Disconnect() },
                {15, new UpdateRank() }
            };
        }

        public void SendCpe() {

        }

        /// <summary>
        /// Handles incoming data from the server
        /// </summary>
        private void Handle() {
            while (true) {
                if (_receiveBuffer.Length == 0)
                    break;

                byte opcode = _receiveBuffer.PeekByte(); // -- Peek doesn't remove the byte. Lets us check if we have enough data yet.
                IPacket packet;
                
                if (!_packets.TryGetValue(opcode, out packet)) {
                    Bot.OnErrorMessage("Received invalid packet from the server.");
                    return;
                }

                if (_receiveBuffer.Length >= packet.PacketLength) { // -- Check if we have enough data to read this packet.
                    _receiveBuffer.ReadByte(); // -- Trim off the opcode, the packet is ready to be read.
                    packet.Read(_receiveBuffer); // -- Read the data from the buffer
                    packet.Handle(this); // -- Handle it.
                }
                else // -- Not enough data, wait for more.
                    break;
            
            }
        }
    }
}