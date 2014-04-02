using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassicBot.World {
    public class WorldContainer {
        public byte[] BlockArray;
        public Vector3s MapSize;

        public WorldContainer() {
            BlockArray = new byte[0];
        }

        /// <summary>
        /// Retreives the block at the given coordinates.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <returns></returns>
        public byte GetBlockId(short X, short Y, short Z) {
            int index = (Z * MapSize.Y + Y) * MapSize.X + X;

            if (index > BlockArray.Length - 1)
                return 0;

            return BlockArray[index];
        }

        /// <summary>
        /// Updates the block at the given coordinates.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <param name="Type"></param>
        public void UpdateBlock(short X, short Y, short Z, byte Type) {
            int index = (Z * MapSize.Y + Y) * MapSize.X + X;

            if (index > BlockArray.Length - 1)
                return;

            BlockArray[index] = Type;
        }

        /// <summary>
        /// Performs a sanity check against the map file to ensure that there are no out of range values.
        /// </summary>
        /// <param name="ClientMain"></param>
        public void WorldCheck(Main ClientMain) {
            for (int x = 0; x < MapSize.X - 1; x++) {
                for (int y = 0; y < MapSize.Y - 1; y++) {
                    for (int z = 0; z < MapSize.Z - 1; z++) {
                        var MyId = GetBlockId((short)x, (short)y, (short)z);
                        if (MyId > 49) {
                            ClientMain.RaiseDebugMessage("Block ID out of bounds: " + MyId.ToString() + " :" + x.ToString() + " " + y.ToString() + " " + z.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the Int at the beginning of the byte array that gives its length.
        /// </summary>
        public void RemoveSize() {
            var Temp = new byte[BlockArray.Length - 4];

            Buffer.BlockCopy(BlockArray, 3, Temp, 0, BlockArray.Length - 4);

            BlockArray = Temp;
            Temp = null;
        }
    }
}
