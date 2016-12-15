using SyncIO.Transport.Encryption;
using SyncIO.Transport.Encryption.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport {
    internal class SyncIOPackConfig {
        public ISyncIOEncryption Encryption { get; set; }
        /// <summary>
        /// Generates a usable SyncIOEncryptionRijndael object
        /// </summary>
        /// <param name="size">Size of encryption key</param>
        /// <returns></returns>
        public static ISyncIOEncryption GenerateNewEncryption(SyncIOKeySize size) {
            var rij = new RijndaelManaged();
            return new SyncIOEncryptionRijndael(SyncIOPackager.RandomBytes((int)size));
        }

        /// <summary>
        /// Manages post packing functions
        /// </summary>
        public byte[] PostPacking(byte[] data) {
            if (Encryption != null)
                data = Encryption.Encrypt(data);

            return data;
        }

        /// <summary>
        /// Manages pre unpacking functions
        /// </summary>
        public byte[] PreUnpacking(byte[] data) {
            if (Encryption != null)
                data = Encryption.Decrypt(data);

            return data;
        }
    }
    /// <summary>
    /// Bit -> byte for encryption keys
    /// </summary>
    internal enum SyncIOKeySize {
        Bit128 = 16,
        Bit192 = 24,
        Bit256 = 32
    }
}
