namespace SyncIO.Transport.Encryption.Defaults
{
    using System;
    using System.Security.Cryptography;

    public class SyncIOEncryptionRC2 : ISyncIOEncryption
    {
        private readonly byte[] _key;
        private readonly byte[] _salt;

        public SyncIOEncryptionRC2(byte[] key, byte[] salt)
        {
            _key = key;
            _salt = salt;
        }

        public byte[] Encrypt(byte[] data)
        {
            return CipherUtility.Encrypt<RC2CryptoServiceProvider>(data, _key, _salt);
        }

        public byte[] Decrypt(byte[] data)
        {
            return CipherUtility.Decrypt<RC2CryptoServiceProvider>(data, _key, _salt);
        }

        public void Dispose()
        {
        }
    }
}