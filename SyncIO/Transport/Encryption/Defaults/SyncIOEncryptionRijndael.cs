namespace SyncIO.Transport.Encryption.Defaults
{
    using System;
    using System.Security.Cryptography;

    public class SyncIOEncryptionRijndael : ISyncIOEncryption
    {
        private readonly Rijndael _rijObj;
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;

        public SyncIOEncryptionRijndael(byte[] key)
        {
            _rijObj = new RijndaelManaged
            {
                Key = key,
                IV = key
            };
            _encryptor = _rijObj.CreateEncryptor();
            _decryptor = _rijObj.CreateDecryptor();
        }

        public SyncIOEncryptionRijndael(Rijndael rijObject)
        {
            _rijObj = rijObject;
            _encryptor = _rijObj.CreateEncryptor();
            _decryptor = _rijObj.CreateDecryptor();
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
            _rijObj.Dispose();
        }
    }
}