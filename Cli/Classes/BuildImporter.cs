using System;
using System.IO;
using ClassicBot.Common;

namespace Cli.Classes {
    public static class BuildImporter {
        private const string ExportDirectory = "Exports";

        /// <summary>
        /// Exports blocks to file from between the given coordinates.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="name"></param>
        /// <param name="bot"></param>
        public static void ExportArea(Vector3S[] coords, string name, ClassicBot.ClassicBot bot) {
            short minX = Math.Min(coords[0].X, coords[1].X);
            short minY = Math.Min(coords[0].Y, coords[1].Y);
            short minZ = Math.Min(coords[0].Z, coords[1].Z);
            short maxX = Math.Max(coords[0].X, coords[1].X);
            short maxY = Math.Max(coords[0].Y, coords[1].Y);
            short maxZ = Math.Max(coords[0].Z, coords[1].Z);
            int sizeX = (maxX - minX) + 1;
            int sizeY = (maxY - minY) + 1;
            int sizeZ = (maxZ - minZ) + 1;
            int indX = 0, indY = 0, indZ = 0;
            var blocks = new byte[(sizeX * sizeY * sizeZ) + 6]; // -- + 6 for the sizes.

            // -- Place the export size up front.
            var xBytes = BitConverter.GetBytes((short) sizeX);
            var yBytes = BitConverter.GetBytes((short) sizeY);
            var zBytes = BitConverter.GetBytes((short) sizeZ);
            Array.Copy(xBytes, blocks, 2);
            Array.Copy(yBytes, 0, blocks, 2, 2);
            Array.Copy(zBytes, 0, blocks, 4, 2);

            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    for (int z = minZ; z <= maxZ; z++) {
                        int blockIndex = ((indZ * sizeY + indY) * sizeX + indX) + 6;

                        blocks[blockIndex] = bot.McClient.ClientPlayer.World.GetBlockId(x, y, z);
                        indZ++;
                    }
                    indZ = 0;
                    indY++;
                }
                indY = 0;
                indX++;
            }
            if (!Directory.Exists(ExportDirectory))
                Directory.CreateDirectory(ExportDirectory);

            File.WriteAllBytes(Path.Combine(ExportDirectory, name + ".mbot"), blocks);
        }

        public static void ImportArea(Vector3S start, string name, ClassicBot.ClassicBot bot) {
            var pathTo = Path.Combine(ExportDirectory, name + ".mbot");
            if (!File.Exists(pathTo)) {
                bot.McClient.ClientPlayer.SendMessage("That export does not exist.");
                return;
            }

            byte[] blocks = File.ReadAllBytes(pathTo);
            int sizeX = 0, sizeY = 0, sizeZ = 0;
            // -- get the sizes out.
            sizeX = BitConverter.ToInt16(blocks, 0);
            sizeY = BitConverter.ToInt16(blocks, 2);
            sizeZ = BitConverter.ToInt16(blocks, 4);

            for (int x = 0; x < sizeX; x++) {
                for (int y =0; y < sizeY; y++) {
                    for (int z = 0; z < sizeZ; z++) {
                        var blockIndex = ((z * sizeY + y) * sizeX + x) + 6;
                        var placeLocation = new Vector3S {
                            X = (short)(start.X + x),
                            Y = (short)(start.Y + y),
                            Z = (short)(start.Z + z)
                        };

                        bot.McClient.ClientPlayer.PlaceBlock(placeLocation, blocks[blockIndex]);
                    }
                }
            }

        }
    }
}