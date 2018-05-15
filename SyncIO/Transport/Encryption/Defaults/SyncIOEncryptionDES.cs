namespace SyncIO.Transport.Encryption.Defaults
{
    using System;
    using System.Security.Cryptography;

    public class SyncIOEncryptionDES : ISyncIOEncryption
    {
        private readonly byte[] _key;
        private readonly byte[] _salt;

        public SyncIOEncryptionDES(byte[] key, byte[] salt)
        {
            _key = key;
            _salt = salt;
        }

        public byte[] Encrypt(byte[] data)
        {
            //System.Security.Cryptography.AesCryptoServiceProvider
            //System.Security.Cryptography.DESCryptoServiceProvider
            //System.Security.Cryptography.DSACryptoServiceProvider
            //System.Security.Cryptography.MD5CryptoServiceProvider
            //System.Security.Cryptography.RC2CryptoServiceProvider
            //System.Security.Cryptography.RNGCryptoServiceProvider
            //System.Security.Cryptography.RSACryptoServiceProvider
            //System.Security.Cryptography.SHA1CryptoServiceProvider
            //System.Security.Cryptography.SHA256CryptoServiceProvider
            //System.Security.Cryptography.SHA384CryptoServiceProvider
            //System.Security.Cryptography.SHA512CryptoServiceProvider
            return CipherUtility.Encrypt<DESCryptoServiceProvider>(data, _key, _salt);
        }

        public byte[] Decrypt(byte[] data)
        {
            return CipherUtility.Decrypt<DESCryptoServiceProvider>(data, _key, _salt);
        }

        public void Dispose()
        {
        }
    }
}