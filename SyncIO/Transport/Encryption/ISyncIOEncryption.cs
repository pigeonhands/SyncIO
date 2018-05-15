namespace SyncIO.Transport.Encryption
{
    using System;

    public interface ISyncIOEncryption : IDisposable
    {
        byte[] Encrypt(byte[] data);

        byte[] Decrypt(byte[] data);
    }
}