using System;
using System.IO;
using System.IO.Compression;

namespace ClassicBot.Common {
    public static class GZip {
        private const int ChunkSize = 65536;

        /// <summary>
        /// Ungzips a gzipped byte array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns>Uncompressed byte array</returns>
        public static byte[] UnGZip(byte[] array) {
            byte[] decompressedData;
            File.WriteAllBytes("dump.gz", array);

            using (var myMem = new MemoryStream()) {
                using (var zip = new GZipStream(new MemoryStream(array), CompressionMode.Decompress)) {
                    var buffer = new byte[ChunkSize];

                    while (true) {
                        int bytesRead = zip.Read(buffer, 0, ChunkSize);

                        if (bytesRead == 0) break;

                        myMem.Write(buffer, 0, bytesRead);
                    }

                    decompressedData = myMem.ToArray();
                }
            }

            GC.Collect();
            return decompressedData;
        }
    }
}
