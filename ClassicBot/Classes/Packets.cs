using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ClassicBot.World;

namespace ClassicBot.Classes {

    interface IPacket {
        byte Id { get; }
        void Read(NetworkManager NM);
        void Write(NetworkManager NM);
        void Handle(NetworkManager NM, Main Core);
    }

    public struct Handshake : IPacket {

        public byte Id { get { return 0; } }
        public byte ProtocolVersion { get; set; }
        public string Name { get; set; }
        public string MOTD { get; set; }
        public byte Usertype { get; set; }

        public void Read(NetworkManager NM) {
            ProtocolVersion = NM.wSock.ReadByte();
            Name = NM.wSock.ReadString();
            MOTD = NM.wSock.ReadString();
            Usertype = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(ProtocolVersion);
                NM.wSock.WriteString(Name);
                NM.wSock.WriteString(MOTD);
                NM.wSock.WriteByte(Usertype);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
        	Core.RaiseInfoMessage("Connected to " + Name);
			Core.RaiseInfoMessage("MOTD: " + MOTD);

			if (Usertype == 100)
				Core.RaiseInfoMessage("You are an operator.");

        }
    }

    public struct Ping : IPacket {
        public byte Id { get { return 1; } }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            Core.LastActive = DateTime.UtcNow;
            Core.RefreshLocation();
            Core.RaisePingReceived();
        }
    }

    public struct LevelInit : IPacket {
        public byte Id { get { return 2; } }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            Core.ClientWorld.BlockArray = new byte[0];
			Core.RaiseInfoMessage("Incoming Level");
            Core.RaiseLevelInit();
        }
    }

    public struct LevelChunk : IPacket {
        public byte Id { get { return 3; } }
        public short Length { get; set; }
        public byte[] Data { get; set; }
        public byte Percent { get; set; }

        public void Read(NetworkManager NM) {
            Length = NM.wSock.ReadShort();
            Data = NM.wSock.ReadByteArray();
            Percent = NM.wSock.ReadByte();
        }
        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteShort(Length);
                NM.wSock.WriteByteArray(Data);
                NM.wSock.WriteByte(Percent);
                NM.wSock.Purge();
            }
        }
        public void Handle(NetworkManager NM, Main Core) {
            var Temp = Core.ClientWorld.BlockArray;
            Core.ClientWorld.BlockArray = new byte[Temp.Length + Length];

			Buffer.BlockCopy(Temp, 0, Core.ClientWorld.BlockArray, 0, Temp.Length);
            Buffer.BlockCopy(Data, 0, Core.ClientWorld.BlockArray, Temp.Length, Length);

			Temp = null;

			Core.RaiseDebugMessage("Map chunk size: " + Length.ToString() + " Percent: " + Percent.ToString());
            Core.RaiseLevelProgress(Percent);
        }
    }

    public struct LevelFinalize : IPacket {
        public byte Id { get { return 4; } }

        public short SizeX { get; set; }
        public short SizeY { get; set; }
        public short SizeZ { get; set; }

        public void Read(NetworkManager NM) {
            SizeX = NM.wSock.ReadShort();
            SizeZ = NM.wSock.ReadShort();
            SizeY = NM.wSock.ReadShort();
        }
        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteShort(SizeX);
                NM.wSock.WriteShort(SizeZ);
                NM.wSock.WriteShort(SizeY);
                NM.wSock.Purge();
            }
        }
        public void Handle(NetworkManager NM, Main Core) {
            Core.ClientWorld.BlockArray = GZip.UnGZip(Core.ClientWorld.BlockArray);

            int BlockArraySize = BitConverter.ToInt32(Core.ClientWorld.BlockArray, 0);
            Core.RaiseDebugMessage("Block array size: " + BlockArraySize.ToString());

            Core.ClientWorld.RemoveSize();

            Core.ClientWorld.MapSize = new Vector3s();
            Core.ClientWorld.MapSize.X = SizeX;
            Core.ClientWorld.MapSize.Y = SizeZ;
            Core.ClientWorld.MapSize.Z = SizeY;

			Core.RaiseInfoMessage("Map complete.");
            Core.RaiseDebugMessage("Level Finalize size: " + SizeX.ToString() + " " + SizeY.ToString() + " " + SizeZ.ToString());
            Core.RaiseLevelComplete(SizeX, SizeY, SizeZ);

            Core.ClientWorld.WorldCheck(Core);
        }
    }

    public struct SetBlock : IPacket {
        public byte Id { get { return 5; } }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte Mode { get; set; }
        public byte Block { get; set; }

        public void Read(NetworkManager NM) {
            X = NM.wSock.ReadShort();
            Z = NM.wSock.ReadShort();
            Y = NM.wSock.ReadShort();
            Mode = NM.wSock.ReadByte();
            Block = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteShort(X);
                NM.wSock.WriteShort(Z);
                NM.wSock.WriteShort(Y);
                NM.wSock.WriteByte(Mode);
                NM.wSock.WriteByte(Block);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            // -- Client to server only
        }
    }

    public struct SetBlockServer : IPacket {
        public byte Id { get { return 6; } }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte Block { get; set; }

        public void Read(NetworkManager NM) {
            X = NM.wSock.ReadShort();
            Z = NM.wSock.ReadShort();
            Y = NM.wSock.ReadShort();
            Block = NM.wSock.ReadByte();
        }
        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteShort(X);
                NM.wSock.WriteShort(Z);
                NM.wSock.WriteShort(Y);
                NM.wSock.WriteByte(Block);
                NM.wSock.Purge();
            }
        }
        public void Handle(NetworkManager NM, Main Core) {
            Core.ClientWorld.UpdateBlock(X, Y, Z, Block);
            Core.RaiseBlockChange(X, Y, Z);
        }
    }

    public struct SpawnPlayer : IPacket {
        public byte Id { get { return 7; } }
        public sbyte PlayerID { get; set; }
        public string PlayerName { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

        public void Read(NetworkManager NM) {
            PlayerID = NM.wSock.ReadSByte();
            PlayerName = NM.wSock.ReadString();
            X = NM.wSock.ReadShort();
            Z = NM.wSock.ReadShort();
            Y = NM.wSock.ReadShort();
            Yaw = NM.wSock.ReadByte();
            Pitch = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteSByte(PlayerID);
                NM.wSock.WriteString(PlayerName);
                NM.wSock.WriteShort(X);
                NM.wSock.WriteShort(Z);
                NM.wSock.WriteShort(Y);
                NM.wSock.WriteByte(Yaw);
                NM.wSock.WriteByte(Pitch);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            if (PlayerID != -1) {
                var NewEnt = new Entity(PlayerName, PlayerID, X, Y, Z, Yaw, Pitch);
                Core.Entities.Add(NewEnt);
                Core.RaisePlayerJoin(NewEnt);
            }
        }
    }

    public struct PlayerTeleport : IPacket {
        public byte Id { get { return 8; } }
        public sbyte PlayerID { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte yaw { get; set; }
        public byte pitch { get; set; }

        public void Read(NetworkManager NM) {
            PlayerID = NM.wSock.ReadSByte();
            X = NM.wSock.ReadShort();
            Z = NM.wSock.ReadShort();
            Y = NM.wSock.ReadShort();
            yaw = NM.wSock.ReadByte();
            pitch = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteSByte(PlayerID);
                NM.wSock.WriteShort(X);
                NM.wSock.WriteShort(Z);
                NM.wSock.WriteShort(Y);
                NM.wSock.WriteByte(yaw);
                NM.wSock.WriteByte(pitch);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            var MyEntity = Entity.GetEntitybyID(PlayerID, Core.Entities);

            if (MyEntity != null) {
                MyEntity.UpdateLocation(X, Y, Z);
                MyEntity.UpdateLook(yaw, pitch);
                Core.RaisePlayerMoved(MyEntity);
            }

            if (PlayerID == -1) {
                Core.Location.X = X;
                Core.Location.Y = Y;
                Core.Location.Z = Z;
                Core.Position[0] = yaw;
                Core.Position[1] = pitch;
                Core.RaiseYouMoved();
            }
        }
    }

    public struct PosAndOrient : IPacket {
        public byte Id { get { return 9; } }
        public sbyte PlayerID { get; set; }
        public short ChangeX { get; set; }
        public short ChangeY { get; set; }
        public short ChangeZ { get; set; }
        public byte yaw { get; set; }
        public byte pitch { get; set; }

        public void Read(NetworkManager NM) {
            PlayerID = NM.wSock.ReadSByte();
            ChangeX = NM.wSock.ReadShort();
            ChangeZ = NM.wSock.ReadShort();
            ChangeY = NM.wSock.ReadShort();
            yaw = NM.wSock.ReadByte();
            pitch = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteSByte(PlayerID);
                NM.wSock.WriteShort(ChangeX);
                NM.wSock.WriteShort(ChangeZ);
                NM.wSock.WriteShort(ChangeY);
                NM.wSock.WriteByte(yaw);
                NM.wSock.WriteByte(pitch);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            var MyEntity = Entity.GetEntitybyID(PlayerID, Core.Entities);

            if (MyEntity != null) {
                MyEntity.UpdateLocation((short)(MyEntity.Location.X + ChangeX), (short)(MyEntity.Location.Y + ChangeY), (short)(MyEntity.Location.Z + ChangeZ));
                MyEntity.UpdateLook(yaw, pitch);
                Core.RaisePlayerMoved(MyEntity);
            }

            if (PlayerID == -1) {
                Core.Location.X += ChangeX;
                Core.Location.Y += ChangeY;
                Core.Location.Z += ChangeZ;
                Core.Position[0] = yaw;
                Core.Position[1] = pitch;
                Core.RaiseYouMoved();
            }
        }
    }

    public struct PositionUpdate : IPacket {
        public byte Id { get { return 10; } }
        public sbyte PlayerID { get; set; }
        public short ChangeX { get; set; }
        public short ChangeY { get; set; }
        public short ChangeZ { get; set; }

        public void Read(NetworkManager NM) {
            PlayerID = NM.wSock.ReadSByte();
            ChangeX = NM.wSock.ReadShort();
            ChangeZ = NM.wSock.ReadShort();
            ChangeY = NM.wSock.ReadShort();
        }

        public void Write(NetworkManager NM) {

        }

        public void Handle(NetworkManager NM, Main Core) {
            var MyEntity = Entity.GetEntitybyID(PlayerID, Core.Entities);

            if (MyEntity != null) {
                MyEntity.UpdateLocation((short)(MyEntity.Location.X + ChangeX), (short)(MyEntity.Location.Y + ChangeY), (short)(MyEntity.Location.Z + ChangeZ));
                Core.RaisePlayerMoved(MyEntity);
            }

            if (PlayerID == -1) {
                Core.Location.X += ChangeX;
                Core.Location.Y += ChangeY;
                Core.Location.Z += ChangeZ;
                Core.RaiseYouMoved();
            }
        }
    }

    public struct OrientationUpdate : IPacket {
        public byte Id { get { return 11; } }
        public sbyte PlayerID { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

        public void Read(NetworkManager NM) {
            PlayerID = NM.wSock.ReadSByte();
            Yaw = NM.wSock.ReadByte();
            Pitch = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {

        }

        public void Handle(NetworkManager NM, Main Core) {
            var MyEntity = Entity.GetEntitybyID(PlayerID, Core.Entities);

            if (MyEntity != null) {
                MyEntity.UpdateLook(Yaw, Pitch);
                Core.RaisePlayerMoved(MyEntity);
            }

            if (PlayerID == -1) {
                Core.Position[0] = Yaw;
                Core.Position[1] = Pitch;
                Core.RaiseYouMoved();
            }
        }
    }

    public struct DespawnPlayer : IPacket {
        public byte Id { get { return 12; } }
        public sbyte PlayerID;

        public void Read(NetworkManager NM) {
            PlayerID = NM.wSock.ReadSByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteSByte(PlayerID);
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            var MyEntity = Entity.GetEntitybyID(PlayerID, Core.Entities);

            if (MyEntity != null) {
                Core.Entities.Remove(MyEntity);
                Core.RaisePlayerLeft(MyEntity);
                
            }
        }
    }

    public struct Message : IPacket {
        public byte Id { get { return 13; } }
        public sbyte PlayerID { get; set; }
        public string Text { get; set; }

        public void Read(NetworkManager NM) {
            PlayerID = NM.wSock.ReadSByte();
            Text = NM.wSock.ReadString();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteSByte(PlayerID);
                NM.wSock.WriteString(Text);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            Core.RaiseChatMessage(Text);
        }
    }

    public struct Disconnect : IPacket {
        public byte Id { get { return 14; } }
        public string Reason { get; set; }

        public void Read(NetworkManager NM) {
            Reason = NM.wSock.ReadString();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteString(Reason);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            Core.RaiseDisconnected(Reason);
        }
    }

    public struct UpdateRank : IPacket {
        public byte Id { get { return 14; } }
        public byte Rank { get; set; }

        public void Read(NetworkManager NM) {
            Rank = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(Rank);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            Core.RaiseAuthChange(Rank);   
        }
    }

    public struct ExtInfo : IPacket {
        public byte Id { get { return 16; } }
        public string AppName { get; set; }
        public short ExtensionCount { get; set; }

        public void Read(NetworkManager NM) {
            AppName = NM.wSock.ReadString();
            ExtensionCount = NM.wSock.ReadShort();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteString(AppName);
                NM.wSock.WriteShort(ExtensionCount);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            Core.SupportsCPE = true;
            Core.ServerAppName = AppName;
            Core.Extensions = ExtensionCount;
            Core.ServerExtensions = new Dictionary<string, int>();
            Core.RaiseDebugMessage("Connecting to " + AppName + " with " + ExtensionCount.ToString() + " extensions.");
        }
    }

    public struct ExtEntry : IPacket {
        public byte Id { get { return 17; } }
        public string ExtName { get; set; }
        public int Version { get; set; }

        public void Read(NetworkManager NM) {
            ExtName = NM.wSock.ReadString();
            Version = NM.wSock.ReadInt();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteString(ExtName);
                NM.wSock.WriteInt(Version);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            Core.ReceivedExtensions += 1;

            if (Core.ReceivedExtensions > Core.Extensions) 
                Core.RaiseInfoMessage("Warning: Server sent more extensions than ExtInfo reported.");

            if (!Core.ServerExtensions.ContainsKey(ExtName))
                Core.ServerExtensions.Add(ExtName, Version);

            Core.RaiseDebugMessage("Received ExtEntry: " + ExtName + " -- " + Version.ToString());

            if (Core.ReceivedExtensions == Core.Extensions) 
                NM.SendCPE();
        }
    }

    public struct SetClickDistance : IPacket {
        public byte Id { get { return 18; } }
        public short Distance { get; set; }

        public void Read(NetworkManager NM) {
            Distance = NM.wSock.ReadShort();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteShort(Distance);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            Core.ClickDistance = Distance;
            Core.RaiseClickDistanceSet(Distance);
        }
    }

    public struct CustomBlockSupportLevel : IPacket {
        public byte Id { get { return 19; } }
        public byte SupportLevel { get; set; }

        public void Read(NetworkManager NM) {
            SupportLevel = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(SupportLevel);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            Core.ServerCBLevel = SupportLevel;

            var myCB = new CustomBlockSupportLevel();
            myCB.SupportLevel = Main.CustomBlockSuportlevel;
            myCB.Write(NM);

            Core.RaiseDebugMessage("Received CustomBlockSupportLevel.");
        }
    }

    public struct HoldThis : IPacket {
        public byte Id { get { return 20; } }
        public byte BlockToHold { get; set; }
        public byte PreventChange { get; set; }

        public void Read(NetworkManager NM) {
            BlockToHold = NM.wSock.ReadByte();
            PreventChange = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(BlockToHold);
                NM.wSock.WriteByte(PreventChange);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            Core.HeldBlock = BlockToHold;
            Core.CanChangeBlock = (PreventChange > 0);
            Core.RaiseHeldBlockChange(BlockToHold, PreventChange);
        }
    }

    public struct SetTextHotKey : IPacket {
        public byte Id { get { return 21; } }
        public string Label { get; set; }
        public string Action { get; set; }
        public int KeyCode { get; set; }
        public byte KeyMods { get; set; }

        public void Read(NetworkManager NM) {
            Label = NM.wSock.ReadString();
            Action = NM.wSock.ReadString();
            KeyCode = NM.wSock.ReadInt();
            KeyMods = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteString(Label);
                NM.wSock.WriteString(Action);
                NM.wSock.WriteInt(KeyCode);
                NM.wSock.WriteByte(KeyMods);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {
            var myHotkey = new TextHotKeyEntry();
            myHotkey.Label = Label;
            myHotkey.Action = Action;
            myHotkey.Keycode = KeyCode;
            myHotkey.Modifier = (HotkeyModifier)KeyMods;

            if (Core.Hotkeys == null)
                Core.Hotkeys = new List<TextHotKeyEntry>();

            Core.Hotkeys.Add(myHotkey);
            Core.RaiseHotkeyAdded();
        }
    }

    public struct ExtAddPlayerName : IPacket {
        public byte Id { get { return 22; } }
        public short NameID { get; set; }
        public string PlayerName { get; set; }
        public string ListName { get; set; }
        public string GroupName { get; set; }
        public byte GroupRank { get; set; }

        public void Read(NetworkManager NM) {
            NameID = NM.wSock.ReadShort();
            PlayerName = NM.wSock.ReadString();
            ListName = NM.wSock.ReadString();
            GroupName = NM.wSock.ReadString();
            GroupRank = NM.wSock.ReadByte();
        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteShort(NameID);
                NM.wSock.WriteString(PlayerName);
                NM.wSock.WriteString(ListName);
                NM.wSock.WriteString(GroupName);
                NM.wSock.WriteByte(GroupRank);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }

    public struct ExtAddEntity : IPacket {
        public byte Id { get { return 23; } }
        public byte EntityID { get; set; }
        public string InGameName { get; set; }
        public string SkinName { get; set; }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(EntityID);
                NM.wSock.WriteString(InGameName);
                NM.wSock.WriteString(SkinName);
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }
    public struct ExtRemovePlayerName : IPacket {
        public byte Id { get { return 24; } }
        public short NameID { get; set; }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteShort(NameID);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }

    public struct EnvSetColor : IPacket {
        public byte Id { get { return 25; } }
        public byte ColorType { get; set; }
        public short Red { get; set; }
        public short Green { get; set; }
        public short Blue { get; set; }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(ColorType);
                NM.wSock.WriteShort(Red);
                NM.wSock.WriteShort(Green);
                NM.wSock.WriteShort(Blue);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }

    public struct MakeSelection : IPacket {
        public byte Id { get { return 26; } }
        public byte SelectionID { get; set; }
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

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(SelectionID);
                NM.wSock.WriteString(Label);
                NM.wSock.WriteShort(StartX);
                NM.wSock.WriteShort(StartZ);
                NM.wSock.WriteShort(StartY);
                NM.wSock.WriteShort(EndX);
                NM.wSock.WriteShort(EndZ);
                NM.wSock.WriteShort(EndY);
                NM.wSock.WriteShort(Red);
                NM.wSock.WriteShort(Green);
                NM.wSock.WriteShort(Blue);
                NM.wSock.WriteShort(Opacity);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }

    public struct RemoveSelection : IPacket {
        public byte Id { get { return 27; } }
        public byte SelectionID { get; set; }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(SelectionID);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }

    public struct SetBlockPermissions : IPacket {
        public byte Id { get { return 28; } }
        public byte BlockType { get; set; }
        public byte AllowPlacement { get; set; }
        public byte AllowDeletion { get; set; }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(BlockType);
                NM.wSock.WriteByte(AllowPlacement);
                NM.wSock.WriteByte(AllowDeletion);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }

    public struct ChangeModel : IPacket {
        public byte Id { get { return 29; } }
        public byte EntityID { get; set; }
        public string ModelName { get; set; }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(EntityID);
                NM.wSock.WriteString(ModelName);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }
    public struct EnvSetMapAppearance : IPacket {
        public byte Id { get { return 30; } }
        public string TextureURL { get; set; }
        public byte SideBlock { get; set; }
        public byte EdgeBlock { get; set; }
        public short SideLevel { get; set; }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteString(TextureURL);
                NM.wSock.WriteByte(SideBlock);
                NM.wSock.WriteByte(EdgeBlock);
                NM.wSock.WriteShort(SideLevel);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }
    public struct EnvSetWeatherType : IPacket {
        public byte Id { get { return 31; } }
        public byte WeatherType { get; set; }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(WeatherType);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }
    public struct HackControl : IPacket {
        public byte Id { get { return 32; } }
        public byte Flying { get; set; }
        public byte NoClip { get; set; }
        public byte Speeding { get; set; }
        public byte SpawnControl { get; set; }
        public byte ThirdPerson { get; set; }
        public short JumpHeight { get; set; }

        public void Read(NetworkManager NM) {

        }

        public void Write(NetworkManager NM) {
            lock (NM.WriteLock) {
                NM.wSock.WriteByte(Id);
                NM.wSock.WriteByte(Flying);
                NM.wSock.WriteByte(NoClip);
                NM.wSock.WriteByte(Speeding);
                NM.wSock.WriteByte(SpawnControl);
                NM.wSock.WriteByte(ThirdPerson);
                NM.wSock.WriteShort(JumpHeight);
                NM.wSock.Purge();
            }
        }

        public void Handle(NetworkManager NM, Main Core) {

        }
    }
}

