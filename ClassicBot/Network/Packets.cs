using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ClassicBot.Common;
using ClassicBot.World;

namespace ClassicBot.Network {
    #region Vanilla
    public struct Handshake : IPacket {
        public static byte Id => 0;
        public int PacketLength => 131;
        public byte ProtocolVersion { get; set; }

        public string Name { get; set; }
        public string Motd { get; set; }
        public byte Usertype { get; set; }

        public void Read(ByteBuffer buf) {
            ProtocolVersion = buf.ReadByte();
            Name = buf.ReadString();
            Motd = buf.ReadString();
            Usertype = buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(ProtocolVersion);
            buf.WriteString(Name); // -- Username
            buf.WriteString(Motd); // -- Verification key (Mppass)
            buf.WriteByte(Usertype); // -- Unused / CPE Support?

        }

        public void Handle(Client c) {
            if (ProtocolVersion != 7) {
                c.Bot.OnErrorMessage("Unsupported protocol version: " + ProtocolVersion);
                c.Shutdown();
                return;
            }

            c.ClientPlayer.Login(Name, Motd, Usertype);
        }
    }

    public struct Ping : IPacket {
        public static byte Id => 1;
        public int PacketLength => 1;

        public void Read(ByteBuffer buf) {

        }
        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id); // -- Note, some (most?) servers don't support client returning a ping with a ping.


        }
        public void Handle(Client c) {
            c.ClientPlayer.RefreshLocation();
        }
    }

    public struct LevelInit : IPacket {
        public static byte Id => 2;
        public int PacketLength => 1;
        public void Read(ByteBuffer buf) {

        }
        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
        }

        public void Handle(Client c) {
            c.ClientPlayer.World.WorldData = new byte[0];
            c.ClientPlayer.OnLevelInitiated();
            c.Bot.OnDebugMessage("Initializing level...");
        }
    }

    public struct LevelChunk : IPacket {
        public static byte Id => 3;
        public int PacketLength => 1028;
        public short Length { get; set; }
        public byte[] Data { get; set; }
        public byte Percent { get; set; }

        public void Read(ByteBuffer buf) {
            Length = buf.ReadShort();
            Data = buf.ReadByteArray();
            Percent = buf.ReadByte();
        }
        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteShort(Length);
            buf.AddBytes(Data);
            buf.WriteByte(Percent);

        }
        public void Handle(Client c) {
            int tLength = c.ClientPlayer.World.WorldData.Length;

            var newData = new byte[tLength + Length];
            Buffer.BlockCopy(c.ClientPlayer.World.WorldData, 0, newData, 0, tLength);
            Buffer.BlockCopy(Data, 0, newData, tLength, Length);
            c.ClientPlayer.World.WorldData = newData;
            c.Bot.OnDebugMessage("Received level chunk: " + Percent + " complete. ");
        }
    }

    public struct LevelFinalize : IPacket {
        public static byte Id => 4;
        public int PacketLength => 7;
        public short SizeX { get; set; }
        public short SizeY { get; set; }
        public short SizeZ { get; set; }

        public void Read(ByteBuffer buf) {
            SizeX = buf.ReadShort();
            SizeZ = buf.ReadShort();
            SizeY = buf.ReadShort();
        }
        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteShort(SizeX);
            buf.WriteShort(SizeZ);
            buf.WriteShort(SizeY);


        }
        public void Handle(Client c) {
            c.ClientPlayer.World.MapSize = new Vector3S(SizeX, SizeY, SizeZ);
            
            byte[] unzipped = GZip.UnGZip(c.ClientPlayer.World.WorldData);
            c.ClientPlayer.World.WorldData = unzipped ??
                                             new byte[(SizeX*SizeY*SizeZ) + 4];
            if (unzipped == null) {
                c.Bot.OnErrorMessage("Failed to ungzip map.");
            }
            else {
                var sizeBytes = new byte[4];
                Buffer.BlockCopy(c.ClientPlayer.World.WorldData, 0, sizeBytes, 0, 4);
                Array.Reverse(sizeBytes);
                int checksumLength = BitConverter.ToInt32(sizeBytes, 0);

                if (checksumLength != (c.ClientPlayer.World.WorldData.Length - 4)) {
                    c.Bot.OnErrorMessage("Map size mismatch.");
                }
                else {
                    c.ClientPlayer.World.WorldCheck(c);
                }
            }

            c.ClientPlayer.World.RemoveSize();
            c.ClientPlayer.OnLevelFinished();
            c.Bot.OnDebugMessage("Level Complete.");
        }
    }

    public struct SetBlock : IPacket {
        public static byte Id => 5;
        public int PacketLength => 9;
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte Mode { get; set; }
        public byte Block { get; set; }

        public void Read(ByteBuffer buf) {
            X = buf.ReadShort();
            Z = buf.ReadShort();
            Y = buf.ReadShort();
            Mode = buf.ReadByte();
            Block = buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteShort(X);
            buf.WriteShort(Z);
            buf.WriteShort(Y);
            buf.WriteByte(Mode);
            buf.WriteByte(Block);
        }

        public void Handle(Client c) {
            // -- Client to server only
        }
    }

    public struct SetBlockServer : IPacket {
        public static byte Id => 6;
        public int PacketLength => 8;
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte Block { get; set; }

        public void Read(ByteBuffer buf) {
            X = buf.ReadShort();
            Z = buf.ReadShort();
            Y = buf.ReadShort();
            Block = buf.ReadByte();
        }
        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteShort(X);
            buf.WriteShort(Z);
            buf.WriteShort(Y);
            buf.WriteByte(Block);


        }
        public void Handle(Client c) {
            if (Block > 49 && !c.SupportsExtension("CustomBlocks", 1)) {
                c.Bot.OnWarningMessage("WARNING: Received CPE Block change with no CustomBlocks support!");
            }

            c.ClientPlayer.World.UpdateBlock(X, Y, Z, Block);
            c.ClientPlayer.OnBlockPlace(new Vector3S(X, Y, Z));
        }
    }

    public struct SpawnPlayer : IPacket {
        public static byte Id => 7;
        public int PacketLength => 74;
        public sbyte PlayerId { get; set; }
        public string PlayerName { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

        public void Read(ByteBuffer buf) {
            PlayerId = (sbyte)buf.ReadByte();
            PlayerName = buf.ReadString();
            X = buf.ReadShort();
            Z = buf.ReadShort();
            Y = buf.ReadShort();
            Yaw = buf.ReadByte();
            Pitch = buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte((byte)PlayerId);
            buf.WriteString(PlayerName);
            buf.WriteShort(X);
            buf.WriteShort(Z);
            buf.WriteShort(Y);
            buf.WriteByte(Yaw);
            buf.WriteByte(Pitch);

        }

        public void Handle(Client c) {
            if (PlayerId == -1) {
                c.ClientPlayer.RefreshLocation(new Vector3S(X, Y, Z), Yaw, Pitch);
                c.Bot.OnDebugMessage("Position set via spawnplayer packet");
                c.ClientPlayer.OnTeleported();
                return;
            }

            var e = new Entity {
                Name = PlayerName,
                PlayerId = PlayerId,
                Position = new Vector3S(X, Y, Z),
                Yaw = Yaw,
                Pitch = Pitch
            };

            c.ClientPlayer.Entities.Add(PlayerId, e);
            c.ClientPlayer.OnEntitySpawn(e);
            c.Bot.OnDebugMessage("Spawning client " + PlayerName + " -- " + PlayerId);
        }
    }

    public struct PlayerTeleport : IPacket {
        public static byte Id => 8;
        public int PacketLength => 10;
        public sbyte PlayerId { get; set; }
        public Vector3S Location { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

        public void Read(ByteBuffer buf) {
            PlayerId = (sbyte)buf.ReadByte();
            Location = new Vector3S { X = buf.ReadShort(), Z = buf.ReadShort(), Y = buf.ReadShort() };
            Yaw = buf.ReadByte();
            Pitch = buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte((byte)PlayerId);
            buf.WriteShort(Location.X);
            buf.WriteShort(Location.Z);
            buf.WriteShort(Location.Y);
            buf.WriteByte(Yaw);
            buf.WriteByte(Pitch);
        }

        public void Handle(Client c) {
            if (PlayerId == -1) {
                c.ClientPlayer.RefreshLocation(Location, Yaw, Pitch);
                c.ClientPlayer.OnTeleported();
                return;
            }

            Entity affectedEntity;

            if (!c.ClientPlayer.Entities.TryGetValue(PlayerId, out affectedEntity)) {
                c.Bot.OnWarningMessage("Server tried to move non-existing player.");
                return;
            }

            affectedEntity.Position = Location;
            affectedEntity.Yaw = Yaw;
            affectedEntity.Pitch = Pitch;
            c.ClientPlayer.OnEntityMove(affectedEntity);
        }
    }

    public struct PosAndOrient : IPacket {
        public static byte Id => 9;
        public int PacketLength => 7;
        public sbyte PlayerId { get; set; }
        public sbyte ChangeX { get; set; }
        public sbyte ChangeY { get; set; }
        public sbyte ChangeZ { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

        public void Read(ByteBuffer buf) {
            PlayerId = (sbyte)buf.ReadByte();
            ChangeX = (sbyte)buf.ReadByte();
            ChangeZ = (sbyte)buf.ReadByte();
            ChangeY = (sbyte)buf.ReadByte();
            Yaw = buf.ReadByte();
            Pitch = buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte((byte)PlayerId);
            buf.WriteShort(ChangeX);
            buf.WriteShort(ChangeZ);
            buf.WriteShort(ChangeY);
            buf.WriteByte(Yaw);
            buf.WriteByte(Pitch);

        }

        public void Handle(Client c) {
            if (PlayerId == -1) {
                
                //c.ClientPlayer.RefreshLocation();
                return;
            }

            Entity affectedEntity;

            if (!c.ClientPlayer.Entities.TryGetValue(PlayerId, out affectedEntity)) {
                c.Bot.OnWarningMessage("Server tried to move non-existing player.");
                return;
            }
            
            affectedEntity.Position.X += (short)(ChangeX);
            affectedEntity.Position.Y += (short) (ChangeY);
            affectedEntity.Position.Z += (short) (ChangeZ);
            affectedEntity.Yaw = Yaw;
            affectedEntity.Pitch = Pitch;
            c.ClientPlayer.OnEntityMove(affectedEntity);
        }
    }

    public struct PositionUpdate : IPacket {
        public static byte Id => 10;
        public int PacketLength => 5;
        public sbyte PlayerId { get; set; }
        public sbyte ChangeX { get; set; }
        public sbyte ChangeY { get; set; }
        public sbyte ChangeZ { get; set; }

        public void Read(ByteBuffer buf) {
            PlayerId = (sbyte)buf.ReadByte();
            ChangeX = (sbyte)buf.ReadByte();
            ChangeZ = (sbyte)buf.ReadByte();
            ChangeY = (sbyte)buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {

        }
        public void Handle(Client c) {
            if (PlayerId == -1) {

                //c.ClientPlayer.RefreshLocation();
                return;
            }

            Entity affectedEntity;

            if (!c.ClientPlayer.Entities.TryGetValue(PlayerId, out affectedEntity)) {
                c.Bot.OnWarningMessage("Server tried to move non-existing player.");
                return;
            }

            affectedEntity.Position.X += (short)(ChangeX);
            affectedEntity.Position.Y += (short)(ChangeY);
            affectedEntity.Position.Z += (short)(ChangeZ);
            c.ClientPlayer.OnEntityMove(affectedEntity);
        }
    }

    public struct OrientationUpdate : IPacket {
        public static byte Id => 11;
        public int PacketLength => 4;
        public sbyte PlayerId { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

        public void Read(ByteBuffer buf) {
            PlayerId = (sbyte)buf.ReadByte();
            Yaw = buf.ReadByte();
            Pitch = buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte((byte)PlayerId);
            buf.WriteByte(Yaw);
            buf.WriteByte(Pitch);

        }

        public void Handle(Client c) {
            if (PlayerId == -1) {

                //c.ClientPlayer.RefreshLocation();
                return;
            }

            Entity affectedEntity;

            if (!c.ClientPlayer.Entities.TryGetValue(PlayerId, out affectedEntity)) {
                c.Bot.OnWarningMessage("Server tried to move non-existing player.");
                return;
            }
            
            affectedEntity.Yaw = Yaw;
            affectedEntity.Pitch = Pitch;
            c.ClientPlayer.OnEntityMove(affectedEntity);
        }
    }

    public struct DespawnPlayer : IPacket {
        public static byte Id => 12;
        public int PacketLength => 2;
        public sbyte PlayerId;

        public void Read(ByteBuffer buf) {
            PlayerId = (sbyte)buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte((byte)PlayerId);
        }

        public void Handle(Client c) {
            if (PlayerId == -1)
                return;

            if (c.ClientPlayer.Entities.ContainsKey(PlayerId)) {
                c.ClientPlayer.OnEntityDespawn(c.ClientPlayer.Entities[PlayerId]);
                c.ClientPlayer.Entities.Remove(PlayerId);
            }
            else
                c.Bot.OnWarningMessage("Server tried to despawn an entity that is not spawned.");
        }
    }

    public struct Message : IPacket {
        public static byte Id => 13;
        public int PacketLength => 66;
        public sbyte PlayerId { get; set; }
        public string Text { get; set; }

        public void Read(ByteBuffer buf) {
            PlayerId = (sbyte)buf.ReadByte();
            Text = buf.ReadString();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte((byte)PlayerId);
            buf.WriteString(Text);

        }

        public void Handle(Client c) {
            c.Bot.OnChatMessage(Text.TrimEnd());
        }
    }

    public struct Disconnect : IPacket {
        public static byte Id => 14;
        public int PacketLength => 65;
        public string Reason { get; set; }

        public void Read(ByteBuffer buf) {
            Reason = buf.ReadString();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteString(Reason);
        }

        public void Handle(Client c) {
            c.Bot.OnErrorMessage("Kicked: " + Reason.Trim());
            c.ClientPlayer.OnKick(Reason.Trim());
        }
    }

    public struct UpdateRank : IPacket {
        public static byte Id => 15;
        public int PacketLength => 2;
        public byte Rank { get; set; }

        public void Read(ByteBuffer buf) {
            Rank = buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(Rank);


        }

        public void Handle(Client c) {
            c.ClientPlayer.UpdateRank(Rank);
        }
    }
    #endregion

    #region CPE
    public struct ExtInfo : IPacket {
        public static byte Id => 16;
        public int PacketLength => 67;
        public string AppName { get; set; }
        public short ExtensionCount { get; set; }

        public void Read(ByteBuffer buf) {
            AppName = buf.ReadString();
            ExtensionCount = buf.ReadShort();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteString(AppName);
            buf.WriteShort(ExtensionCount);
        }

        public void Handle(Client c) {
            if (!c.IsCpe)
                c.Bot.OnErrorMessage("WARNING: Received CPE Handshake while not supporting CPE!");

            c.IsCpe = true;
            c.ServerAppName = AppName;
            c.ServerExtensionCount = ExtensionCount;
            c.Extensions = new Dictionary<string, int>();
            c.Bot.OnInfoMessage("Server Application: " + AppName + ", supporting " + ExtensionCount + " CPE Extensions.");
        }
    }

    public struct ExtEntry : IPacket {
        public static byte Id => 17;
        public int PacketLength => 69;
        public string ExtName { get; set; }
        public int Version { get; set; }

        public void Read(ByteBuffer buf) {
            ExtName = buf.ReadString();
            Version = buf.ReadInt();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteString(ExtName);
            buf.WriteInt(Version);
        }

        public void Handle(Client c) {
            if (!c.IsCpe)
                c.Bot.OnWarningMessage("WARNING: Received CPE Extension while not supporting CPE!");

            if (c.SentCpe)
                c.Bot.OnWarningMessage("WARNING: Received CPE Extension after sending client extensions!");

            if (c.Extensions.Count > c.ServerExtensionCount)
                c.Bot.OnWarningMessage("WARNING: Received more CPE Extensions than server reported!");

            if (!c.Extensions.ContainsKey(ExtName))
                c.Extensions.Add(ExtName, Version);
            else
                c.Bot.OnWarningMessage("WARNING: Server sent same extension multiple times! - " + ExtName);

            if (c.Extensions.Count == c.ServerExtensionCount)
                c.SendCpe();
        }
    }

    public struct SetClickDistance : IPacket {
        public byte Id => 18;
        public int PacketLength => 3;
        public short Distance { get; set; }

        public void Read(ByteBuffer buf) {
            Distance = buf.ReadShort();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteShort(Distance);
        }

        public void Handle(Client c) {

        }
    }

    public struct CustomBlockSupportLevel : IPacket {
        public static byte Id => 19;
        public int PacketLength => 2;
        public byte SupportLevel { get; set; }

        public void Read(ByteBuffer buf) {
            SupportLevel = buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(SupportLevel);
        }

        public void Handle(Client c) {
        }
    }

    public struct HoldThis : IPacket {
        public byte Id => 20;
        public int PacketLength => 3;
        public byte BlockToHold { get; set; }
        public byte PreventChange { get; set; }

        public void Read(ByteBuffer buf) {
            BlockToHold = buf.ReadByte();
            PreventChange = buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(BlockToHold);
            buf.WriteByte(PreventChange);


        }

        public void Handle(Client c) {

        }
    }

    public struct SetTextHotKey : IPacket {
        public byte Id => 21;
        public int PacketLength => 134;
        public string Label { get; set; }
        public string Action { get; set; }
        public int KeyCode { get; set; }
        public byte KeyMods { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteString(Label);
            buf.WriteString(Action);
            buf.WriteInt(KeyCode);
            buf.WriteByte(KeyMods);
        }

        public void Handle(Client c) {

        }
    }

    public struct ExtAddPlayerName : IPacket {
        public byte Id => 22;
        public int PacketLength => 196;
        public short NameId { get; set; }
        public string PlayerName { get; set; }
        public string ListName { get; set; }
        public string GroupName { get; set; }
        public byte GroupRank { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteShort(NameId);
            buf.WriteString(PlayerName);
            buf.WriteString(ListName);
            buf.WriteString(GroupName);
            buf.WriteByte(GroupRank);


        }

        public void Handle(Client c) {

        }
    }

    public struct ExtAddEntity : IPacket {
        public byte Id => 23;
        public int PacketLength => 130;
        public byte EntityId { get; set; }
        public string InGameName { get; set; }
        public string SkinName { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(EntityId);
            buf.WriteString(InGameName);
            buf.WriteString(SkinName);

        }

        public void Handle(Client c) {

        }
    }
    public struct ExtRemovePlayerName : IPacket {
        public byte Id => 24;
        public int PacketLength => 3;
        public short NameId { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteShort(NameId);


        }

        public void Handle(Client c) {

        }
    }

    public struct EnvSetColor : IPacket {
        public byte Id => 25;
        public int PacketLength => 8;
        public byte ColorType { get; set; }
        public short Red { get; set; }
        public short Green { get; set; }
        public short Blue { get; set; }

        public enum ColorTypes {
            SkyColor = 0,
            CloudColor,
            FogColor,
            AmbientColor,
            SunlightColor,
        }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(ColorType);
            buf.WriteShort(Red);
            buf.WriteShort(Green);
            buf.WriteShort(Blue);


        }

        public void Handle(Client c) {

        }
    }

    public struct MakeSelection : IPacket {
        public byte Id => 26;
        public int PacketLength => 86;
        public byte SelectionId { get; set; }
        public string Label { get; set; }
        public short StartX { get; set; }
        public short StartY { get; set; }
        public short StartZ { get; set; }
        public short EndX { get; set; }
        public short EndY { get; set; }
        public short EndZ { get; set; }
        public short Red { get; set; }
        public short Green { get; set; }
        public short Blue { get; set; }
        public short Opacity { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(SelectionId);
            buf.WriteString(Label);
            buf.WriteShort(StartX);
            buf.WriteShort(StartZ);
            buf.WriteShort(StartY);
            buf.WriteShort(EndX);
            buf.WriteShort(EndZ);
            buf.WriteShort(EndY);
            buf.WriteShort(Red);
            buf.WriteShort(Green);
            buf.WriteShort(Blue);
            buf.WriteShort(Opacity);


        }

        public void Handle(Client c) {

        }
    }

    public struct RemoveSelection : IPacket {
        public byte Id => 27;
        public int PacketLength => 2;
        public byte SelectionId { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(SelectionId);


        }

        public void Handle(Client c) {

        }
    }

    public struct SetBlockPermissions : IPacket {
        public byte Id => 28;
        public int PacketLength => 4;
        public byte BlockType { get; set; }
        public byte AllowPlacement { get; set; }
        public byte AllowDeletion { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(BlockType);
            buf.WriteByte(AllowPlacement);
            buf.WriteByte(AllowDeletion);


        }

        public void Handle(Client c) {

        }
    }

    public struct ChangeModel : IPacket {
        public byte Id => 29;
        public int PacketLength => 66;
        public byte EntityId { get; set; }
        public string ModelName { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(EntityId);
            buf.WriteString(ModelName);


        }

        public void Handle(Client c) {

        }
    }
    public struct EnvSetMapAppearance : IPacket {
        public byte Id => 30;
        public int PacketLength => 69;
        public string TextureUrl { get; set; }
        public byte SideBlock { get; set; }
        public byte EdgeBlock { get; set; }
        public short SideLevel { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteString(TextureUrl);
            buf.WriteByte(SideBlock);
            buf.WriteByte(EdgeBlock);
            buf.WriteShort(SideLevel);


        }

        public void Handle(Client c) {

        }
    }

    public struct EnvSetMapAppearance2 : IPacket {
        public byte Id => 30;
        public int PacketLength => 73;
        public string TexturePackUrl { get; set; }
        public byte SideBlock { get; set; }
        public byte EdgeBlock { get; set; }
        public short SideLevel { get; set; }
        public short CloudLevel { get; set; }
        public short ViewDistance { get; set; }
        public void Read(ByteBuffer buf) {
            throw new NotImplementedException();
        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteString(TexturePackUrl);
            buf.WriteByte(SideBlock);
            buf.WriteByte(EdgeBlock);
            buf.WriteShort(SideLevel);
            buf.WriteShort(CloudLevel);
            buf.WriteShort(ViewDistance);
        }

        public void Handle(Client client) {
            throw new NotImplementedException();
        }
    }
    public struct EnvSetWeatherType : IPacket {
        public byte Id => 31;
        public int PacketLength => 2;
        public byte WeatherType { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(WeatherType);


        }

        public void Handle(Client c) {

        }
    }

    public struct HackControl : IPacket {
        public byte Id => 32;
        public int PacketLength => 8;
        public byte Flying { get; set; }
        public byte NoClip { get; set; }
        public byte Speeding { get; set; }
        public byte SpawnControl { get; set; }
        public byte ThirdPerson { get; set; }
        public short JumpHeight { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(Flying);
            buf.WriteByte(NoClip);
            buf.WriteByte(Speeding);
            buf.WriteByte(SpawnControl);
            buf.WriteByte(ThirdPerson);
            buf.WriteShort(JumpHeight);


        }

        public void Handle(Client c) {

        }
    }

    public struct ExtAddEntity2 : IPacket {
        public byte Id => 33;
        public int PacketLength => 138;
        public byte EntityId { get; set; }
        public string InGameName { get; set; }
        public string SkinName { get; set; }
        public Vector3S Spawn { get; set; }
        public byte SpawnYaw { get; set; }
        public byte SpawnPitch { get; set; }

        public void Read(ByteBuffer buf) {

        }

        public void Write(ByteBuffer buf) {
            buf.WriteByte(Id);
            buf.WriteByte(EntityId);
            buf.WriteString(InGameName);
            buf.WriteString(SkinName);
            buf.WriteShort(Spawn.X);
            buf.WriteShort(Spawn.Z);
            buf.WriteShort(Spawn.Y);
            buf.WriteByte(SpawnYaw);
            buf.WriteByte(SpawnPitch);

        }

        public void Handle(Client c) {

        }
    }

    public struct PlayerClicked : IPacket {
        public byte Id => 34;
        public int PacketLength => 15;
        public byte Button { get; set; }
        public byte Action { get; set; }
        public short Yaw { get; set; }
        public short Pitch { get; set; }
        public byte TargetEntityId { get; set; }
        public short TargetBlockX { get; set; }
        public short TargetBlockY { get; set; }
        public short TargetBlockZ { get; set; }
        public byte TargetBlockFace { get; set; }

        public void Read(ByteBuffer buf) {
            Button = buf.ReadByte();
            Action = buf.ReadByte();
            Yaw = buf.ReadShort();
            Pitch = buf.ReadShort();
            TargetEntityId = buf.ReadByte();
            TargetBlockX = buf.ReadShort();
            TargetBlockZ = buf.ReadShort();
            TargetBlockY = buf.ReadShort();
            TargetBlockFace = buf.ReadByte();
        }

        public void Write(ByteBuffer buf) {
            throw new NotImplementedException();
        }

        public void Handle(Client buf) {
            throw new NotImplementedException();
        }
    }

    public struct DefineBlock {

    }

    public struct RemoveBlockDefinition {

    }

    public struct DefineBlockExt {

    }

    public struct SetTextColor {

    }

    public struct BulkBlockUpdate {

    }
    #endregion

}
