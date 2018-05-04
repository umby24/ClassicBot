using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicBot.Common;
using ClassicBot.Network;

namespace ClassicBot.World {
    public class RemoteWorld {
        public string Name { get; set; }
        public string Motd { get; set; }
        public Vector3S MapSize;
        public byte[] WorldData;

        /// <summary>
        /// Retreives the block at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public byte GetBlockId(short x, short y, short z) {
            int index = (z * MapSize.Y + y) * MapSize.X + x;

            return (byte)(index > WorldData.Length - 1 ? 0 : WorldData[index]);
        }
        
        public byte GetBlockId(int x, int y, int z) {
            int index = (z * MapSize.Y + y) * MapSize.X + x;

            return (byte)(index > WorldData.Length - 1 ? 0 : WorldData[index]);
        }
        
        public byte GetBlockId(Vector3S location) {
            int index = (location.Z * MapSize.Y + location.Y) * MapSize.X + location.X;

            return (byte)(index > WorldData.Length - 1 ? 0 : WorldData[index]);
        }

        /// <summary>
        /// Updates the block at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="type"></param>
        internal void UpdateBlock(short x, short y, short z, byte type) {
            int index = (z * MapSize.Y + y) * MapSize.X + x;

            if (index > WorldData.Length - 1)
                return;

            WorldData[index] = type;
        }

        /// <summary>
        /// Performs a sanity check against the map file to ensure that there are no out of range values.
        /// </summary>
        /// <param name="c"></param>
        public void WorldCheck(Client c) {
            for (var x = 0; x < MapSize.X - 1; x++) {
                for (var y = 0; y < MapSize.Y - 1; y++) {
                    for (var z = 0; z < MapSize.Z - 1; z++) {
                        var myId = GetBlockId((short)x, (short)y, (short)z);
                        if (myId > 49 && !c.SupportsExtension("Custom Blocks", 1)) {
                            Console.WriteLine("Block ID Outside of bounds: " + myId);
                      //      clientBot.RaiseErrorMessage("Block ID out of bounds: " + myId + " :" + x + " " + y + " " + z);
                        }
                        else if (myId > 65) {
                            Console.WriteLine("Block ID Outside of bounds: " + myId);
                            //         clientBot.RaiseErrorMessage("Block ID out of bounds: " + myId + " :" + x + " " + y + " " + z);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the Int at the beginning of the byte array that gives its length.
        /// </summary>
        internal void RemoveSize() {
            var temp = new byte[WorldData.Length - 4];

            Buffer.BlockCopy(WorldData, 3, temp, 0, WorldData.Length - 4);

            WorldData = temp;
        }
        
        
    }
}
