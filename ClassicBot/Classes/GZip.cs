using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zlib;

namespace ClassicBot.Classes {
    class GZip {
        const int ChunkSize = 65536;

        /// <summary>
        /// Ungzips a gzipped byte array.
        /// </summary>
        /// <param name="Array"></param>
        /// <returns>Uncompressed byte array</returns>
        public static byte[] UnGZip(byte[] Array) {
            byte[] DecompressedData;

            using (var MyMem = new MemoryStream()) {
                using (var zip = new GZipStream(new MemoryStream(Array), CompressionMode.Decompress)) {
                    var Buffer = new byte[ChunkSize];

                    while (true) {
                        int BytesRead = zip.Read(Buffer, 0, ChunkSize);

                        if (BytesRead == 0) break;

                        MyMem.Write(Buffer, 0, BytesRead);
                    }

                    Buffer = null;
                    DecompressedData = MyMem.ToArray();
                }
            }

            GC.Collect();
            return DecompressedData;
        }
    }
}
