using System;
using System.Diagnostics;
using System.IO;
using Ionic.Zlib;

namespace ClassicBot.Classes {
    class GZip {
        const int ChunkSize = 65536;

        /// <summary>
        /// Ungzips a gzipped byte array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns>Uncompressed byte array</returns>
        public static byte[] UnGZip(byte[] array) {
            byte[] decompressedData = null;

            using (var myMem = new MemoryStream()) {
                try {
                    using (var zip = new GZipStream(new MemoryStream(array), CompressionMode.Decompress)) {
                        var buffer = new byte[ChunkSize];

                        while (true) {
                            int bytesRead = zip.Read(buffer, 0, ChunkSize);

                            if (bytesRead == 0) break;

                            myMem.Write(buffer, 0, bytesRead);
                        }

                        buffer = null;
                        decompressedData = myMem.ToArray();
                    }
                }
                catch {
                    Debug.Print("Suppressed GZIP Exception");
                }
            }

            GC.Collect();
            return decompressedData;
        }
    }
}
