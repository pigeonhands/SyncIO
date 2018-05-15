namespace SyncIO.Transport.Encryption.Defaults
{
    using System;
    using System.Security.Cryptography;

    public class SyncIOEncryptionAes : ISyncIOEncryption
    {
        private readonly byte[] _key;
        private readonly byte[] _salt;

        public SyncIOEncryptionAes(byte[] key, byte[] salt)
        {
            _key = key;
            _salt = salt;
        }

        public byte[] Encrypt(byte[] data)
        {
            return CipherUtility.Encrypt<AesCryptoServiceProvider>(data, _key, _salt);
        }

        public byte[] Decrypt(byte[] data)
        {
            return CipherUtility.Decrypt<AesCryptoServiceProvider>(data, _key, _salt);
        }

        public void Dispose()
        {
        }
    }
}