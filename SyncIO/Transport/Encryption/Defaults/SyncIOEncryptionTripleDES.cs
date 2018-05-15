namespace SyncIO.Transport.Encryption.Defaults
{
    using System;
    using System.Security.Cryptography;

    public class SyncIOEncryptionTripleDES : ISyncIOEncryption
    {
        private readonly byte[] _key;
        private readonly byte[] _salt;

        public SyncIOEncryptionTripleDES(byte[] key, byte[] salt)
        {
            _key = key;
            _salt = salt;
        }

        public byte[] Encrypt(byte[] data)
        {
            return CipherUtility.Encrypt<TripleDESCryptoServiceProvider>(data, _key, _salt);
        }

        public byte[] Decrypt(byte[] data)
        {
            return CipherUtility.Decrypt<TripleDESCryptoServiceProvider>(data, _key, _salt);
        }

        public void Dispose()
        {
        }
    }
}