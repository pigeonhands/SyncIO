namespace SyncIO.Transport.Compression.Defaults
{
    using System;
    using System.IO;
    using System.IO.Compression;

    public class SyncIOCompressionGZip : ISyncIOCompression
    {
        public byte[] Compress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    gzip.Write(data, 0, data.Length);
                }

                Console.WriteLine($"Original size: {data.Length:N0}, Compressed size: {ms.Length:N0}");
                return ms.ToArray();
            }
        }

        public byte[] Decompress(byte[] data)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (var gzip = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
            {
                const int size = 4096;
                var buffer = new byte[size];
                using (var ms = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = gzip.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            ms.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);

                    Console.WriteLine($"Compressed size {data.Length:N0}, decompressed size {ms.Length:N0}.");
                    return ms.ToArray();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}