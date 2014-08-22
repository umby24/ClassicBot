using System;

namespace ClassicBot.World {
    public class WorldContainer {
        public byte[] BlockArray;
        public Vector3S MapSize;

        public WorldContainer() {
            BlockArray = new byte[0];
        }

        /// <summary>
        /// Retreives the block at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public byte GetBlockId(short x, short y, short z) {
            int index = (z * MapSize.Y + y) * MapSize.X + x;

            if (index > BlockArray.Length - 1)
                return 0;

            return BlockArray[index];
        }

        /// <summary>
        /// Updates the block at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="type"></param>
        public void UpdateBlock(short x, short y, short z, byte type) {
            int index = (z * MapSize.Y + y) * MapSize.X + x;

            if (index > BlockArray.Length - 1)
                return;

            BlockArray[index] = type;
        }

        /// <summary>
        /// Performs a sanity check against the map file to ensure that there are no out of range values.
        /// </summary>
        /// <param name="clientBot"></param>
        public void WorldCheck(Bot clientBot) {
            for (var x = 0; x < MapSize.X - 1; x++) {
                for (var y = 0; y < MapSize.Y - 1; y++) {
                    for (var z = 0; z < MapSize.Z - 1; z++) {
                        var myId = GetBlockId((short)x, (short)y, (short)z);
                        if (myId > 49 && !clientBot.ClientSupportedExtensions.Contains(CPEExtensions.CustomBlocks)) {
                            clientBot.RaiseErrorMessage("Block ID out of bounds: " + myId + " :" + x + " " + y + " " + z);
                        } else if (myId > 65) {
                            clientBot.RaiseErrorMessage("Block ID out of bounds: " + myId + " :" + x + " " + y + " " + z);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the Int at the beginning of the byte array that gives its length.
        /// </summary>
        public void RemoveSize() {
            var temp = new byte[BlockArray.Length - 4];

            Buffer.BlockCopy(BlockArray, 3, temp, 0, BlockArray.Length - 4);

            BlockArray = temp;
            temp = null;
        }
    }
}
