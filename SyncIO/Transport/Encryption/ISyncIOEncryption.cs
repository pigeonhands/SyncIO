using System;

namespace SyncIO.Transport.Encryption {
    public interface ISyncIOEncryption : IDisposable {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }
}
