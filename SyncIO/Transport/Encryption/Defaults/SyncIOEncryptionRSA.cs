namespace SyncIO.Transport.Encryption.Defaults
{
    using System;
    using System.Security.Cryptography;

    public class SyncIOEncryptionRSA : ISyncIOEncryption
    {
        private readonly RSACryptoServiceProvider _rsa;

        public byte[] PublicKey { get; }

        public SyncIOEncryptionRSA()
        {
            _rsa = new RSACryptoServiceProvider();
            PublicKey = _rsa.ExportCspBlob(false);
        }

        /// <summary>
        /// Encrypt only.
        /// </summary>
        /// <param name="publicKey">Pubic key for encryption</param>
        public SyncIOEncryptionRSA(byte[] publicKey)
        {
            _rsa = new RSACryptoServiceProvider();
            _rsa.ImportCspBlob(publicKey);
        }

        public byte[] Encrypt(byte[] data)
        {
            return _rsa.Encrypt(data, false);
        }

        public byte[] Decrypt(byte[] data)
        {
            return _rsa.Decrypt(data, false);
        }

        public void Dispose()
        {
            _rsa.Dispose();
        }
    }
}