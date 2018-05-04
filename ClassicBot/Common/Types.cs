using System;
using System.Collections.Generic;
using ClassicBot.Network;
using ClassicBot.World;
using Newtonsoft.Json;

namespace ClassicBot.Common {
    public struct ClassicubeServer : IComparable<ClassicubeServer> {
        public string Hash;
        public string Ip;
        [JsonProperty("players")]
        public int OnlinePlayers;
        public int MaxPlayers;
        public string Mppass;
        public string Name;
        public int Port;
        public string Software;
        public long Uptime;

        public int CompareTo(ClassicubeServer other) {
            return OnlinePlayers.CompareTo(other.OnlinePlayers);
        }
    }

    public class ServersResponse {
        public List<ClassicubeServer> Servers { get; set; }
    }

    public class LoginResponse {
        public bool Authenticated { get; set; }
        public int ErrorCount { get; set; }
        public string[] Errors { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
    }

    public delegate void EmptyEventArgs();

    public delegate void ByteEventArgs(byte value);

    public delegate void Vector3SEventArgs(Vector3S value);

    public delegate void StringEventArgs(string value);
    public delegate void EntityEventArgs(Entity value);
    
    public interface IPacket {
        int PacketLength { get; }
        void Read(ByteBuffer client);
        void Write(ByteBuffer client);
        void Handle(Client client);
    }

    public class Vector3S {
        public Vector3S AsCopy() {
            return new Vector3S(this);
        }
        public bool Equals(Vector3S other) {
            if (other == null)
                return false;
            
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vector3S && Equals((Vector3S)obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public short X;
        public short Y;
        public short Z;

        public Vector3S() {
            
        }
        protected Vector3S(Vector3S other) {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }

        public Vector3S(short x, short y, short z) {
            X = x;
            Y = y;
            Z = z;
        }

        //public static bool operator ==(Vector3S item1, Vector3S item2) {
        //    return item1.X == item2.X && item1.Y == item2.Y && item1.Z == item2.Z;
        //}

        //public static bool operator !=(Vector3S item1, Vector3S item2) {
        //    return !(item1 == item2);
        //}
    }
}
