using System;
using System.Collections.Generic;
using ClassicBot.World;

namespace ClassicBot.Classes {

    interface IPacket {
        byte Id { get; }
        void Read(NetworkManager nm);
        void Write(NetworkManager nm);
        void Handle(NetworkManager nm, Bot core);
    }

    public struct Handshake : IPacket {

        public byte Id { get { return 0; } }
        public byte ProtocolVersion { get; set; }
        public string Name { get; set; }
        public string Motd { get; set; }
        public byte Usertype { get; set; }

        public void Read(NetworkManager nm) {
            ProtocolVersion = nm.wSock.ReadByte();
            Name = nm.wSock.ReadString();
            Motd = nm.wSock.ReadString();
            Usertype = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(ProtocolVersion);
                nm.wSock.WriteString(Name);
                nm.wSock.WriteString(Motd);
                nm.wSock.WriteByte(Usertype);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
        	core.RaiseInfoMessage("Connected to " + Name);
			core.RaiseInfoMessage("MOTD: " + Motd);

			if (Usertype >= 100)
				core.RaiseInfoMessage("You are an operator.");

        }
    }

    public struct Ping : IPacket {
        public byte Id { get { return 1; } }

        public void Read(NetworkManager nm) {

        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            core.LastActive = DateTime.UtcNow;
            core.RefreshLocation();
            core.RaisePingReceived();
        }
    }

    public struct LevelInit : IPacket {
        public byte Id { get { return 2; } }

        public void Read(NetworkManager nm) {

        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            core.ClientWorld.BlockArray = new byte[0];
			core.RaiseInfoMessage("Incoming Level");
            core.RaiseLevelInit();
        }
    }

    public struct LevelChunk : IPacket {
        public byte Id { get { return 3; } }
        public short Length { get; set; }
        public byte[] Data { get; set; }
        public byte Percent { get; set; }

        public void Read(NetworkManager nm) {
            Length = nm.wSock.ReadShort();
            Data = nm.wSock.ReadByteArray();
            Percent = nm.wSock.ReadByte();
        }
        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteShort(Length);
                nm.wSock.WriteByteArray(Data);
                nm.wSock.WriteByte(Percent);
                nm.wSock.Purge();
            }
        }
        public void Handle(NetworkManager nm, Bot core) {
            var temp = core.ClientWorld.BlockArray;
            core.ClientWorld.BlockArray = new byte[temp.Length + Length];

			Buffer.BlockCopy(temp, 0, core.ClientWorld.BlockArray, 0, temp.Length);
            Buffer.BlockCopy(Data, 0, core.ClientWorld.BlockArray, temp.Length, Length);

			core.RaiseDebugMessage("Map chunk size: " + Length + " Percent: " + Percent);
            core.RaiseLevelProgress(Percent);
        }
    }

    public struct LevelFinalize : IPacket {
        public byte Id { get { return 4; } }

        public short SizeX { get; set; }
        public short SizeY { get; set; }
        public short SizeZ { get; set; }

        public void Read(NetworkManager nm) {
            SizeX = nm.wSock.ReadShort();
            SizeZ = nm.wSock.ReadShort();
            SizeY = nm.wSock.ReadShort();
        }
        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteShort(SizeX);
                nm.wSock.WriteShort(SizeZ);
                nm.wSock.WriteShort(SizeY);
                nm.wSock.Purge();
            }
        }
        public void Handle(NetworkManager nm, Bot core) {
            core.ClientWorld.BlockArray = GZip.UnGZip(core.ClientWorld.BlockArray);

            if (core.ClientWorld.BlockArray == null) {
                core.ClientWorld.BlockArray = new byte[(SizeX * SizeY * SizeZ) + 4];
            }
            var blockArraySize = BitConverter.ToInt32(new[] { core.ClientWorld.BlockArray[3], core.ClientWorld.BlockArray[2], core.ClientWorld.BlockArray[1], core.ClientWorld.BlockArray[0] }, 0);
            core.ClientWorld.RemoveSize();

            core.ClientWorld.MapSize = new Vector3S {X = SizeX, Y = SizeZ, Z = SizeY};

            core.RaiseInfoMessage("Map complete.");

            if ((SizeX * SizeY * SizeZ) != blockArraySize)
                core.RaiseErrorMessage(string.Format("Protocol Error: Map data length != Finalize length ({0} - {1})", blockArraySize, (SizeX * SizeY * SizeZ)));

            core.RaiseLevelComplete(SizeX, SizeY, SizeZ);

            //core.ClientWorld.WorldCheck(core);
        }
    }

    public struct SetBlock : IPacket {
        public byte Id { get { return 5; } }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte Mode { get; set; }
        public byte Block { get; set; }

        public void Read(NetworkManager nm) {
            X = nm.wSock.ReadShort();
            Z = nm.wSock.ReadShort();
            Y = nm.wSock.ReadShort();
            Mode = nm.wSock.ReadByte();
            Block = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteShort(X);
                nm.wSock.WriteShort(Z);
                nm.wSock.WriteShort(Y);
                nm.wSock.WriteByte(Mode);
                nm.wSock.WriteByte(Block);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            // -- Client to server only
        }
    }

    public struct SetBlockServer : IPacket {
        public byte Id { get { return 6; } }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte Block { get; set; }

        public void Read(NetworkManager nm) {
            X = nm.wSock.ReadShort();
            Z = nm.wSock.ReadShort();
            Y = nm.wSock.ReadShort();
            Block = nm.wSock.ReadByte();
        }
        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteShort(X);
                nm.wSock.WriteShort(Z);
                nm.wSock.WriteShort(Y);
                nm.wSock.WriteByte(Block);
                nm.wSock.Purge();
            }
        }
        public void Handle(NetworkManager nm, Bot core) {
            core.ClientWorld.UpdateBlock(X, Y, Z, Block);
            core.RaiseBlockChange(X, Y, Z);
        }
    }

    public struct SpawnPlayer : IPacket {
        public byte Id { get { return 7; } }
        public sbyte PlayerId { get; set; }
        public string PlayerName { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

        public void Read(NetworkManager nm) {
            PlayerId = nm.wSock.ReadSByte();
            PlayerName = nm.wSock.ReadString();
            X = nm.wSock.ReadShort();
            Z = nm.wSock.ReadShort();
            Y = nm.wSock.ReadShort();
            Yaw = nm.wSock.ReadByte();
            Pitch = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteSByte(PlayerId);
                nm.wSock.WriteString(PlayerName);
                nm.wSock.WriteShort(X);
                nm.wSock.WriteShort(Z);
                nm.wSock.WriteShort(Y);
                nm.wSock.WriteByte(Yaw);
                nm.wSock.WriteByte(Pitch);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (PlayerId != -1) {
                var newEnt = new Entity(PlayerName, PlayerId, X, Y, Z, Yaw, Pitch);
                core.Entities.Add(PlayerId, newEnt);
                core.RaisePlayerJoin(newEnt);
            }
            else {
                core.Location.X = X;
                core.Location.Y = Y;
                core.Location.Z = Z;
                core.Position[0] = Yaw;
                core.Position[1] = Pitch;
                core.RaiseDebugMessage("Set Location Via SpawnPlayer");
                core.RaiseYouMoved();
            }
        }
    }

    public struct PlayerTeleport : IPacket {
        public byte Id { get { return 8; } }
        public sbyte PlayerId { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

        public void Read(NetworkManager nm) {
            PlayerId = nm.wSock.ReadSByte();
            X = nm.wSock.ReadShort();
            Z = nm.wSock.ReadShort();
            Y = nm.wSock.ReadShort();
            Yaw = nm.wSock.ReadByte();
            Pitch = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteSByte(PlayerId);
                nm.wSock.WriteShort(X);
                nm.wSock.WriteShort(Z);
                nm.wSock.WriteShort(Y);
                nm.wSock.WriteByte(Yaw);
                nm.wSock.WriteByte(Pitch);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.Entities.ContainsKey(PlayerId)) {
                core.RaiseErrorMessage("Protocol Warning: Teleported non-existant player");
                return;
            }

            if (PlayerId != -1) {
                core.Entities[PlayerId].UpdateLocation(X, Y, Z);
                core.Entities[PlayerId].UpdateLook(Yaw, Pitch);
                core.RaisePlayerMoved(core.Entities[PlayerId]);
                return;
            }

            core.Location.X = X;
            core.Location.Y = Y;
            core.Location.Z = Z;
            core.Position[0] = Yaw;
            core.Position[1] = Pitch;
            core.RaiseYouMoved();
            core.RaiseDebugMessage("Set location via Teleport.");
        }
    }

    public struct PosAndOrient : IPacket {
        public byte Id { get { return 9; } }
        public sbyte PlayerId { get; set; }
        public short ChangeX { get; set; }
        public short ChangeY { get; set; }
        public short ChangeZ { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

        public void Read(NetworkManager nm) {
            PlayerId = nm.wSock.ReadSByte();
            ChangeX = nm.wSock.ReadShort();
            ChangeZ = nm.wSock.ReadShort();
            ChangeY = nm.wSock.ReadShort();
            Yaw = nm.wSock.ReadByte();
            Pitch = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteSByte(PlayerId);
                nm.wSock.WriteShort(ChangeX);
                nm.wSock.WriteShort(ChangeZ);
                nm.wSock.WriteShort(ChangeY);
                nm.wSock.WriteByte(Yaw);
                nm.wSock.WriteByte(Pitch);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.Entities.ContainsKey(PlayerId)) {
                core.RaiseErrorMessage("Protocol Warning: Updated Pos+look of non-existant player");
                return;
            }

            if (PlayerId != -1) {
                var myEntity = core.Entities[PlayerId];
                myEntity.UpdateLocation((short)(myEntity.Location.X + ChangeX), (short)(myEntity.Location.Y + ChangeY), (short)(myEntity.Location.Z + ChangeZ));
                myEntity.UpdateLook(Yaw, Pitch);
                core.RaisePlayerMoved(myEntity);
                return;
            }

            core.Location.X += ChangeX;
            core.Location.Y += ChangeY;
            core.Location.Z += ChangeZ;
            core.Position[0] = Yaw;
            core.Position[1] = Pitch;
            core.RaiseYouMoved();
        }
    }

    public struct PositionUpdate : IPacket {
        public byte Id { get { return 10; } }
        public sbyte PlayerId { get; set; }
        public short ChangeX { get; set; }
        public short ChangeY { get; set; }
        public short ChangeZ { get; set; }

        public void Read(NetworkManager nm) {
            PlayerId = nm.wSock.ReadSByte();
            ChangeX = nm.wSock.ReadShort();
            ChangeZ = nm.wSock.ReadShort();
            ChangeY = nm.wSock.ReadShort();
        }

        public void Write(NetworkManager nm) {

        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.Entities.ContainsKey(PlayerId)) {
                core.RaiseErrorMessage("Protocol Warning: Updated position of non-existant player");
                return;
            }

            var myEntity = core.Entities[PlayerId];

            if (PlayerId != -1) {
                myEntity.UpdateLocation((short)(myEntity.Location.X + ChangeX), (short)(myEntity.Location.Y + ChangeY), (short)(myEntity.Location.Z + ChangeZ));
                core.RaisePlayerMoved(myEntity);
            }

            core.Location.X += ChangeX;
            core.Location.Y += ChangeY;
            core.Location.Z += ChangeZ;
            core.RaiseYouMoved();
        }
    }

    public struct OrientationUpdate : IPacket {
        public byte Id { get { return 11; } }
        public sbyte PlayerId { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }

        public void Read(NetworkManager nm) {
            PlayerId = nm.wSock.ReadSByte();
            Yaw = nm.wSock.ReadByte();
            Pitch = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {

        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.Entities.ContainsKey(PlayerId)) {
                core.RaiseErrorMessage("Protocol Warning: Updated orientation of non-existant player");
                return;
            }

            var myEntity = core.Entities[PlayerId];

            if (PlayerId != -1) {
                myEntity.UpdateLook(Yaw, Pitch);
                core.RaisePlayerMoved(myEntity);
                return;
                
            }

            core.Position[0] = Yaw;
            core.Position[1] = Pitch;
            core.RaiseYouMoved();
        }
    }

    public struct DespawnPlayer : IPacket {
        public byte Id { get { return 12; } }
        public sbyte PlayerId;

        public void Read(NetworkManager nm) {
            PlayerId = nm.wSock.ReadSByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteSByte(PlayerId);
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.Entities.ContainsKey(PlayerId)) {
                core.RaiseErrorMessage("Protocol Warning: Despawned non-existant entity.");
                return;
                
            }
            var myEntity = core.Entities[PlayerId];

            core.Entities.Remove(PlayerId);
            core.RaisePlayerLeft(myEntity);
            
        }
    }

    public struct Message : IPacket {
        public byte Id { get { return 13; } }
        public sbyte PlayerId { get; set; }
        public string Text { get; set; }

        public void Read(NetworkManager nm) {
            PlayerId = nm.wSock.ReadSByte();
            Text = nm.wSock.ReadString();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteSByte(PlayerId);
                nm.wSock.WriteString(Text);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            core.RaiseChatMessage(Text);
        }
    }

    public struct Disconnect : IPacket {
        public byte Id { get { return 14; } }
        public string Reason { get; set; }

        public void Read(NetworkManager nm) {
            Reason = nm.wSock.ReadString();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteString(Reason);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            core.RaiseDisconnected(Reason);
        }
    }

    public struct UpdateRank : IPacket {
        public byte Id { get { return 14; } }
        public byte Rank { get; set; }

        public void Read(NetworkManager nm) {
            Rank = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(Rank);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            core.RaiseAuthChange(Rank);   
        }
    }

    public struct ExtInfo : IPacket {
        public byte Id { get { return 16; } }
        public string AppName { get; set; }
        public short ExtensionCount { get; set; }

        public void Read(NetworkManager nm) {
            AppName = nm.wSock.ReadString();
            ExtensionCount = nm.wSock.ReadShort();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteString(AppName);
                nm.wSock.WriteShort(ExtensionCount);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received Extinfo while CPE Disabled.");

            core.SupportsCpe = true;
            core.ServerAppName = AppName;
            core.Extensions = ExtensionCount;
            core.ServerExtensions = new Dictionary<string, int>();
            core.RaiseDebugMessage("Connecting to " + AppName + " with " + ExtensionCount + " extensions.");
        }
    }

    public struct ExtEntry : IPacket {
        public byte Id { get { return 17; } }
        public string ExtName { get; set; }
        public int Version { get; set; }

        public void Read(NetworkManager nm) {
            ExtName = nm.wSock.ReadString();
            Version = nm.wSock.ReadInt();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteString(ExtName);
                nm.wSock.WriteInt(Version);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received Extentry while CPE Disabled.");

            if (core.SentCPE)
                core.RaiseErrorMessage("Protocol error: Received ExtEntry after sending client extensions!");

            core.ReceivedExtensions += 1;

            if (core.ReceivedExtensions > core.Extensions)
                core.RaiseInfoMessage("Warning: Server sent more extensions than ExtInfo reported.");

            if (!core.ServerExtensions.ContainsKey(ExtName))
                core.ServerExtensions.Add(ExtName, Version);
            else 
                core.RaiseErrorMessage("Protocol warning: Server sent ExtEntry of same name multiple times (" + ExtName + ")");
            
            core.RaiseDebugMessage("Received ExtEntry: " + ExtName + " -- " + Version);

            if (core.ReceivedExtensions == core.Extensions) 
                nm.SendCPE();
        }
    }

    public struct SetClickDistance : IPacket {
        public byte Id { get { return 18; } }
        public short Distance { get; set; }

        public void Read(NetworkManager nm) {
            Distance = nm.wSock.ReadShort();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteShort(Distance);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received SetClickDistance while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.ClickDistance))
                core.RaiseErrorMessage("Protocol error: Received SetClickDistance, which client does not support.");

            core.ClickDistance = Distance;
            core.RaiseClickDistanceSet(Distance);
        }
    }

    public struct CustomBlockSupportLevel : IPacket {
        public byte Id { get { return 19; } }
        public byte SupportLevel { get; set; }

        public void Read(NetworkManager nm) {
            SupportLevel = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(SupportLevel);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received CBSL while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.CustomBlocks))
                core.RaiseErrorMessage("Protocol error: Received CBSL, which client does not support.");

            core.ServerCbLevel = SupportLevel;

            var myCb = new CustomBlockSupportLevel {SupportLevel = Bot.CustomBlockSuportlevel};
            myCb.Write(nm);

            core.RaiseDebugMessage("Received CustomBlockSupportLevel.");
        }
    }

    public struct HoldThis : IPacket {
        public byte Id { get { return 20; } }
        public byte BlockToHold { get; set; }
        public byte PreventChange { get; set; }

        public void Read(NetworkManager nm) {
            BlockToHold = nm.wSock.ReadByte();
            PreventChange = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(BlockToHold);
                nm.wSock.WriteByte(PreventChange);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received Holdthis while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.HeldBlock))
                core.RaiseErrorMessage("Protocol error: Received HoldThis, which client does not support.");

            core.HeldBlock = BlockToHold;
            core.CanChangeBlock = (PreventChange > 0);
            core.RaiseHeldBlockChange(BlockToHold, PreventChange);
        }
    }

    public struct SetTextHotKey : IPacket {
        public byte Id { get { return 21; } }
        public string Label { get; set; }
        public string Action { get; set; }
        public int KeyCode { get; set; }
        public byte KeyMods { get; set; }

        public void Read(NetworkManager nm) {
            Label = nm.wSock.ReadString();
            Action = nm.wSock.ReadString();
            KeyCode = nm.wSock.ReadInt();
            KeyMods = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteString(Label);
                nm.wSock.WriteString(Action);
                nm.wSock.WriteInt(KeyCode);
                nm.wSock.WriteByte(KeyMods);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received SetTextHotKey while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.CustomBlocks))
                core.RaiseErrorMessage("Protocol error: Received SetTextHotKey, which client does not support.");

            var myHotkey = new TextHotKeyEntry {
                Label = Label,
                Action = Action,
                Keycode = KeyCode,
                Modifier = (HotkeyModifier) KeyMods
            };

            if (core.Hotkeys == null)
                core.Hotkeys = new List<TextHotKeyEntry>();

            core.Hotkeys.Add(myHotkey);
            core.RaiseHotkeyAdded();
        }
    }

    public struct ExtAddPlayerName : IPacket {
        public byte Id { get { return 22; } }
        public short NameId { get; set; }
        public string PlayerName { get; set; }
        public string ListName { get; set; }
        public string GroupName { get; set; }
        public byte GroupRank { get; set; }

        public void Read(NetworkManager nm) {
            NameId = nm.wSock.ReadShort();
            PlayerName = nm.wSock.ReadString();
            ListName = nm.wSock.ReadString();
            GroupName = nm.wSock.ReadString();
            GroupRank = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteShort(NameId);
                nm.wSock.WriteString(PlayerName);
                nm.wSock.WriteString(ListName);
                nm.wSock.WriteString(GroupName);
                nm.wSock.WriteByte(GroupRank);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received ExtAddPlayerName while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.ExtPlayerList))
                core.RaiseErrorMessage("Protocol error: Received ExtPlayerList packet, which client does not support.");

            var newEntry = new ExtPlayerListEntry {
                NameId = NameId,
                PlayerName = PlayerName,
                ListName = ListName,
                GroupName = GroupName,
                GroupRank = GroupRank
            };

            if (!core.NumberPlayerList.ContainsKey(NameId)) {
                core.ExtPlayerList.Add(PlayerName, newEntry);
                core.NumberPlayerList.Add(NameId, newEntry);
            }
            else {
                core.ExtPlayerList.Remove(core.NumberPlayerList[NameId].PlayerName);
                core.ExtPlayerList.Add(PlayerName, newEntry);
                core.NumberPlayerList[NameId] = newEntry;
            }

            core.RaiseExtPlayerListUpdate();
        }
    }

    public struct ExtAddEntity : IPacket {
        public byte Id { get { return 23; } }
        public byte EntityId { get; set; }
        public string InGameName { get; set; }
        public string SkinName { get; set; }

        public void Read(NetworkManager nm) {
            EntityId = nm.wSock.ReadByte();
            InGameName = nm.wSock.ReadString();
            SkinName = nm.wSock.ReadString();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(EntityId);
                nm.wSock.WriteString(InGameName);
                nm.wSock.WriteString(SkinName);
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received ExtAddEntity while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.ExtPlayerList))
                core.RaiseErrorMessage("Protocol error: Received ExtPlayerList packet, which client does not support.");
        }
    }
    public struct ExtRemovePlayerName : IPacket {
        public byte Id { get { return 24; } }
        public short NameId { get; set; }

        public void Read(NetworkManager nm) {
            NameId = nm.wSock.ReadShort();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteShort(NameId);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received ExtRemovePlayerName while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.ExtPlayerList))
                core.RaiseErrorMessage("Protocol error: Received ExtPlayerList packet, which client does not support.");

            if (!core.NumberPlayerList.ContainsKey(NameId)) {
                core.RaiseErrorMessage("Protocol error: Received ExtRemovePlayerName for non-existant Name ID.");
            }
            else {
                core.ExtPlayerList.Remove(core.NumberPlayerList[NameId].PlayerName);
                core.NumberPlayerList.Remove(NameId);
                core.RaiseExtPlayerListUpdate();
            }
        }
    }

    public struct EnvSetColor : IPacket {
        public byte Id { get { return 25; } }
        public byte ColorType { get; set; }
        public short Red { get; set; }
        public short Green { get; set; }
        public short Blue { get; set; }

        public void Read(NetworkManager nm) {
            ColorType = nm.wSock.ReadByte();
            Red = nm.wSock.ReadShort();
            Green = nm.wSock.ReadShort();
            Blue = nm.wSock.ReadShort();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(ColorType);
                nm.wSock.WriteShort(Red);
                nm.wSock.WriteShort(Green);
                nm.wSock.WriteShort(Blue);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received EnvSetColor while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.EnvColors))
                core.RaiseErrorMessage("Protocol error: Received EnvColors packet, which client does not support.");
        }
    }

    public struct MakeSelection : IPacket {
        public byte Id { get { return 26; } }
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

        public void Read(NetworkManager nm) {
            SelectionId = nm.wSock.ReadByte();
            Label = nm.wSock.ReadString();
            StartX = nm.wSock.ReadShort();
            StartZ = nm.wSock.ReadShort();
            StartY = nm.wSock.ReadShort();
            EndX = nm.wSock.ReadShort();
            EndZ = nm.wSock.ReadShort();
            EndY = nm.wSock.ReadShort();
            Red = nm.wSock.ReadShort();
            Green = nm.wSock.ReadShort();
            Blue = nm.wSock.ReadShort();
            Opacity = nm.wSock.ReadShort();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(SelectionId);
                nm.wSock.WriteString(Label);
                nm.wSock.WriteShort(StartX);
                nm.wSock.WriteShort(StartZ);
                nm.wSock.WriteShort(StartY);
                nm.wSock.WriteShort(EndX);
                nm.wSock.WriteShort(EndZ);
                nm.wSock.WriteShort(EndY);
                nm.wSock.WriteShort(Red);
                nm.wSock.WriteShort(Green);
                nm.wSock.WriteShort(Blue);
                nm.wSock.WriteShort(Opacity);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received MakeSelection while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.SelectionCuboid))
                core.RaiseErrorMessage("Protocol error: Received SelectionCuboid packet, which client does not support.");
        }
    }

    public struct RemoveSelection : IPacket {
        public byte Id { get { return 27; } }
        public byte SelectionId { get; set; }

        public void Read(NetworkManager nm) {
            SelectionId = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(SelectionId);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received RemoveSelection while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.SelectionCuboid))
                core.RaiseErrorMessage("Protocol error: Received SelectionCuboid packet, which client does not support.");
        }
    }

    public struct SetBlockPermissions : IPacket {
        public byte Id { get { return 28; } }
        public byte BlockType { get; set; }
        public byte AllowPlacement { get; set; }
        public byte AllowDeletion { get; set; }

        public void Read(NetworkManager nm) {
            BlockType = nm.wSock.ReadByte();
            AllowPlacement = nm.wSock.ReadByte();
            AllowDeletion = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(BlockType);
                nm.wSock.WriteByte(AllowPlacement);
                nm.wSock.WriteByte(AllowDeletion);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received SetBlockPermissions while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.BlockPermissions))
                core.RaiseErrorMessage("Protocol error: Received BlockPermissions packet, which client does not support.");

            if (BlockType > 49 && !core.ClientSupportedExtensions.Contains(CPEExtensions.CustomBlocks))
                core.RaiseErrorMessage("Protocol error: SetBlockPermissions on block > 49 while custom blocks not supported.");
        }
    }

    public struct ChangeModel : IPacket {
        public byte Id { get { return 29; } }
        public byte EntityId { get; set; }
        public string ModelName { get; set; }

        public void Read(NetworkManager nm) {
            EntityId = nm.wSock.ReadByte();
            ModelName = nm.wSock.ReadString();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(EntityId);
                nm.wSock.WriteString(ModelName);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received ChangeModel while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.ChangeModel))
                core.RaiseErrorMessage("Protocol error: Received ChangeModel packet, which client does not support.");
        }
    }
    public struct EnvSetMapAppearance : IPacket {
        public byte Id { get { return 30; } }
        public string TextureUrl { get; set; }
        public byte SideBlock { get; set; }
        public byte EdgeBlock { get; set; }
        public short SideLevel { get; set; }

        public void Read(NetworkManager nm) {
            TextureUrl = nm.wSock.ReadString();
            SideBlock = nm.wSock.ReadByte();
            EdgeBlock = nm.wSock.ReadByte();
            SideLevel = nm.wSock.ReadShort();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteString(TextureUrl);
                nm.wSock.WriteByte(SideBlock);
                nm.wSock.WriteByte(EdgeBlock);
                nm.wSock.WriteShort(SideLevel);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received EnvSetMapAppearance while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.EnvMapAppearance))
                core.RaiseErrorMessage("Protocol error: Received EnvMapAppearances packet, which client does not support.");
        }
    }
    public struct EnvSetWeatherType : IPacket {
        public byte Id { get { return 31; } }
        public byte WeatherType { get; set; }

        public void Read(NetworkManager nm) {
            WeatherType = nm.wSock.ReadByte();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(WeatherType);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received EnvSetWeatherType while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.EnvWeatherType))
                core.RaiseErrorMessage("Protocol error: Received EnvWeatherType packet, which client does not support.");
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

        public void Read(NetworkManager nm) {
            Flying = nm.wSock.ReadByte();
            NoClip = nm.wSock.ReadByte();
            Speeding = nm.wSock.ReadByte();
            SpawnControl = nm.wSock.ReadByte();
            ThirdPerson = nm.wSock.ReadByte();
            JumpHeight = nm.wSock.ReadShort();
        }

        public void Write(NetworkManager nm) {
            lock (nm.WriteLock) {
                nm.wSock.WriteByte(Id);
                nm.wSock.WriteByte(Flying);
                nm.wSock.WriteByte(NoClip);
                nm.wSock.WriteByte(Speeding);
                nm.wSock.WriteByte(SpawnControl);
                nm.wSock.WriteByte(ThirdPerson);
                nm.wSock.WriteShort(JumpHeight);
                nm.wSock.Purge();
            }
        }

        public void Handle(NetworkManager nm, Bot core) {
            if (!core.EnableCpe)
                core.RaiseErrorMessage("Protocol error: Received HackControl while CPE Disabled.");

            if (!core.ClientSupportedExtensions.Contains(CPEExtensions.HackControl))
                core.RaiseErrorMessage("Protocol error: Received HackControl packet, which client does not support.");
        }
    }
}

