namespace SyncIO.Transport.Encryption.Defaults
{
    using System;
    using System.Security.Cryptography;

    public class SyncIOEncryptionRijndael : ISyncIOEncryption
    {
        private readonly Rijndael _rijndael;
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;

        public SyncIOEncryptionRijndael(byte[] key)
        {
            _rijndael = new RijndaelManaged
            {
                Key = key,
                IV = key
            };
            _encryptor = _rijndael.CreateEncryptor();
            _decryptor = _rijndael.CreateDecryptor();
        }

        public SyncIOEncryptionRijndael(Rijndael rijObject)
        {
            _rijndael = rijObject;
            _encryptor = _rijndael.CreateEncryptor();
            _decryptor = _rijndael.CreateDecryptor();
        }

        public byte[] Decrypt(byte[] data)
        {
            return _decryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public byte[] Encrypt(byte[] data)
        {
            return _encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public void Dispose()
        {
            _encryptor.Dispose();
            _decryptor.Dispose();
            _rijndael.Dispose();
        }
    }
}