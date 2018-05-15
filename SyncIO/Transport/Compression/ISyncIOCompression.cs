namespace SyncIO.Transport.Compression
{
    using System;

    public interface ISyncIOCompression : IDisposable
    {
        byte[] Compress(byte[] data);

        byte[] Decompress(byte[] data);
    }
}