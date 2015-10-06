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
			try {
				byte[] decompressedData = ZlibCodecDecompress (array);
	            return decompressedData;
			} catch (Exception e) {
				Debug.WriteLine (e.Message);
				Debug.WriteLine (e.StackTrace);
				return null;
			}
        }

		private static byte[] ZlibCodecDecompress(byte[] compressed)
		{
			int outputSize = 2048;
			byte[] output = new Byte[ outputSize ];

			// If you have a ZLIB stream, set this to true.  If you have
			// a bare DEFLATE stream, set this to false.
			bool expectRfc1950Header = false;

			using ( MemoryStream ms = new MemoryStream())
			{
				ZlibCodec compressor = new ZlibCodec();
				compressor.InitializeInflate(expectRfc1950Header);

				compressor.InputBuffer = compressed;
				compressor.AvailableBytesIn = compressed.Length;
				compressor.NextIn = 0;
				compressor.OutputBuffer = output;

				foreach (var f in new FlushType[] { FlushType.None, FlushType.Finish } )
				{
					int bytesToWrite = 0;
					do
					{
						compressor.AvailableBytesOut = outputSize;
						compressor.NextOut = 0;
						compressor.Inflate(f);

						bytesToWrite = outputSize - compressor.AvailableBytesOut ;
						if (bytesToWrite > 0)
							ms.Write(output, 0, bytesToWrite);
					}
					while (( f == FlushType.None && (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0)) ||
						( f == FlushType.Finish && bytesToWrite != 0));
				}

				compressor.EndInflate();

				return ms.ToArray();
			}
		}
    }


}
