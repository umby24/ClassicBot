using System;
using System.IO;
using System.IO.Compression;

namespace ClassicBot.Classes {
    class GZip {
        const int ChunkSize = 65536;

        /// <summary>
        /// Ungzips a gzipped byte array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns>Uncompressed byte array</returns>
        public static byte[] UnGZip(byte[] array) {
            byte[] decompressedData;

            using (var myMem = new MemoryStream()) {
                using (var zip = new GZipStream(new MemoryStream(array), CompressionMode.Decompress)) {
                    var buffer = new byte[ChunkSize];

                    while (true) {
                        var bytesRead = zip.Read(buffer, 0, ChunkSize);

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
