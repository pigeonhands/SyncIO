using System.Security.Cryptography;

namespace SyncIO.Transport.Encryption.Defaults {
    public class SyncIOEncryptionRijndael : ISyncIOEncryption {
        private Rijndael RijObject;
        private ICryptoTransform Encryptor;
        private ICryptoTransform Decryptor;
        public SyncIOEncryptionRijndael(byte[] _key) {
            RijObject = new RijndaelManaged();
            RijObject.Key = _key;
            RijObject.IV = _key;
            Encryptor = RijObject.CreateEncryptor();
            Decryptor = RijObject.CreateDecryptor();
        }

        public SyncIOEncryptionRijndael(Rijndael _RijObject) {
            RijObject = _RijObject;
            Encryptor = RijObject.CreateEncryptor();
            Decryptor = RijObject.CreateDecryptor();
        }

        public byte[] Decrypt(byte[] data) {
            return Decryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public byte[] Encrypt(byte[] data) {
            return Encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public void Dispose() {
            Encryptor.Dispose();
            Decryptor.Dispose();
            RijObject.Dispose();
        }
    }
}
