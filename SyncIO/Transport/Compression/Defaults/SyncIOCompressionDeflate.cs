namespace SyncIO.Transport.Compression.Defaults
{
    using System;
    using System.IO;
    using System.IO.Compression;

    public class SyncIOCompressionDeflate : ISyncIOCompression
    {
        public byte[] Compress(byte[] data)
        {
            // Get the stream of the source file. 
            using (var ms = new MemoryStream(data))
            {
                using (var outStream = new MemoryStream())
                {
                    using (var deflate = new DeflateStream(outStream, CompressionMode.Compress))
                    {
                        // Copy the source file into the compression stream.
                        const int size = 4096;
                        var buffer = new byte[size];
                        var numRead = 0;

                        while ((numRead = ms.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            deflate.Write(buffer, 0, numRead);
                        }

                        Console.WriteLine("Compressed from {0} to {1} bytes.", data.Length, ms.Length);
                    }
                    return outStream.ToArray();
                }
            }
        }

        public byte[] Decompress(byte[] data)
        {
            // Get the stream of the source file. 
            using (var ms = new MemoryStream(data))
            {
                using (var outStream = new MemoryStream())
                {
                    using (var deflate = new DeflateStream(ms, CompressionMode.Decompress))
                    {
                        //Copy the decompression stream into the output file.
                        const int size = 4096;
                        var buffer = new byte[size];
                        var numRead = 0;
                        while ((numRead = deflate.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            outStream.Write(buffer, 0, numRead);
                        }
                        Console.WriteLine("Decompressed from {0} to {1} bytes", data.Length, outStream.Length);
                    }
                    return outStream.ToArray();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}