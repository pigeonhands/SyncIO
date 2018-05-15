namespace SyncIO.Transport.Encryption.Defaults
{
    using System;
    using System.Security.Cryptography;

    public class SyncIOEncryptionRSA : ISyncIOEncryption
    {
        private readonly RSACryptoServiceProvider _rsaObj;

        public byte[] PublicKey { get; }

        public SyncIOEncryptionRSA()
        {
            _rsaObj = new RSACryptoServiceProvider();
            PublicKey = _rsaObj.ExportCspBlob(false);
        }

        /// <summary>
        /// Encrypt only.
        /// </summary>
        /// <param name="publicKey">Pubic key for encryption</param>
        public SyncIOEncryptionRSA(byte[] publicKey)
        {
            _rsaObj = new RSACryptoServiceProvider();
            _rsaObj.ImportCspBlob(publicKey);
        }

        public byte[] Encrypt(byte[] data)
        {
            return _rsaObj.Encrypt(data, false);
        }

        public byte[] Decrypt(byte[] data)
        {
            return _rsaObj.Decrypt(data, false);
        }

        public void Dispose()
        {
            _rsaObj.Dispose();
        }
    }
}