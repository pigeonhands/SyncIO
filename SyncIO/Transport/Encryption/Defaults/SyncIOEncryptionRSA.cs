using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.Encryption.Defaults {
    public class SyncIOEncryptionRSA : ISyncIOEncryption {
        public byte[] PublicKey { get; }

        private RSACryptoServiceProvider RSAObj;

        public SyncIOEncryptionRSA() {
            RSAObj = new RSACryptoServiceProvider();
            PublicKey = RSAObj.ExportCspBlob(false);
        }

        /// <summary>
        /// Encrypt only.
        /// </summary>
        /// <param name="_PublicKey">Pubic key for encryption</param>
        public SyncIOEncryptionRSA(byte[] _PublicKey) {
            RSAObj = new RSACryptoServiceProvider();
            RSAObj.ImportCspBlob(_PublicKey);
        }

        public byte[] Encrypt(byte[] data) {
            return RSAObj.Encrypt(data, false);
        }

        public byte[] Decrypt(byte[] data) {
            return RSAObj.Decrypt(data, false);
        }

        public void Dispose() {
            RSAObj.Dispose();
        }
    }
}
