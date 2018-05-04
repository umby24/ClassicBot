using System.Collections.Generic;
using System.IO.Pipes;
using ClassicBot.Common;
using ClassicBot.Network;

namespace ClassicBot.World {
    public class Player {
        public Dictionary<sbyte, Entity> Entities;
        public List<byte> DisallowedBlocks;
        public RemoteWorld World;
        public bool LogInComplete;

        private byte _userType;
        private string _username;
        private string _mppass;
        private Client _client;
        private Entity _thisEntity;
        
        public Player(string username, Client c, string mppass = "") {
            _username = username;
            _client = c;
            _mppass = mppass;
            DisallowedBlocks = new List<byte>();
            _thisEntity = new Entity();
        }

        internal void Login(string name, string motd, byte userType) {
            _userType = userType;
            
            Entities = new Dictionary<sbyte, Entity>();

            World = new RemoteWorld {
                Name = name,
                Motd = motd
            };
            
            LoggedIn?.Invoke();
            LogInComplete = true;

            if (userType >= 99) // -- Checks if this user is an 'op'
                return;

            if (!DisallowedBlocks.Contains(7)) // -- If the user is NOT an 'op', they cannot place bedrock. (block ID 7)
                DisallowedBlocks.Add(7);
        }

        internal void UpdateRank(byte userType) {
            _userType = userType;
            
            if (userType >= 99 && DisallowedBlocks.Contains(7)) {
                DisallowedBlocks.Remove(7);
            }

            if (userType < 99 && !DisallowedBlocks.Contains(7)) { // -- If the user is NOT an 'op', they cannot place bedrock. (block ID 7)
                DisallowedBlocks.Add(7);
            }
            
            RankUpdated?.Invoke(userType); // -- Run the 'Rank Updated' event.
        }

        internal void SendHandshake() {
            var hs = new Handshake {
                Name = _username,
                Motd = _mppass,
                ProtocolVersion = 7,
                Usertype = (byte)(_client.IsCpe ? 66 : 0)
            };

            _client.SendPacket(hs);
        }

        public void RefreshLocation() {
            if (!LogInComplete) {
                _client.Bot.OnErrorMessage("Protocol violation: Location refresh before logged in.");
                return;
            }

            var tp = new PlayerTeleport {
                Location = _thisEntity.Position,
                Yaw = _thisEntity.Yaw,
                Pitch = _thisEntity.Pitch,
                PlayerId = -1
            };

            _client.SendPacket(tp);
        }

        public void RefreshLocation(Vector3S location, byte yaw, byte pitch) {
            location = location.AsCopy();
            _thisEntity.Position = location;
            _thisEntity.Yaw = yaw;
            _thisEntity.Pitch = pitch;
            RefreshLocation();
        }

        public void SendMessage(string message) {
            var msg = new Message {
                Text = message,
                PlayerId = 0
            };
            
            _client.SendPacket(msg);
        }

        public void PlaceBlock(Vector3S location, byte type) {
            var place = new SetBlock {
                Block = type,
                Mode = 1,
                X = location.X,
                Y = location.Y,
                Z = location.Z
            };
            
            World.UpdateBlock(location.X, location.Y, location.Z, type);
            _client.SendPacket(place);
        }

        #region Events

        public event EmptyEventArgs LoggedIn;
        
        public event ByteEventArgs RankUpdated;
        
        public event EntityEventArgs Teleported;
        public event EntityEventArgs EntityMoved;
        public event EntityEventArgs EntitySpawned;
        public event EntityEventArgs EntityDespawned;

        public event StringEventArgs Kicked;
        
        
        public event EmptyEventArgs LevelInitiated;
        public event EmptyEventArgs LevelFinished;
        public event Vector3SEventArgs BlockPlaced;
        
        internal void OnEntityMove(Entity val) {
            EntityMoved?.Invoke(val);
        }

        internal void OnEntitySpawn(Entity val) {
            EntitySpawned?.Invoke(val);
        }

        internal void OnEntityDespawn(Entity val) {
            EntityDespawned?.Invoke(val);
        }

        internal void OnKick(string value) {
            Kicked?.Invoke(value);
        }

        internal void OnTeleported() {
            Teleported?.Invoke(_thisEntity);
        }
                
        internal void OnLevelInitiated() {
            LevelInitiated?.Invoke();
        }

        internal void OnLevelFinished() {
            LevelFinished?.Invoke();
        }

        internal void OnBlockPlace(Vector3S location) {
            BlockPlaced?.Invoke(location);
        }

        #endregion
    }
}
