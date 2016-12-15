using SyncIO.Transport.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport {
    internal class SyncIOPackConfig {
        private ISyncIOEncryption Encryption;
        public void SetEncryption(ISyncIOEncryption enc) {
            Encryption = enc;
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
}
