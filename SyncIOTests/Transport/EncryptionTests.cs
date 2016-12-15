using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncIO.Transport.Encryption.Defaults;
using SyncIO.Transport;
using System.Security.Cryptography;

namespace SyncIOTests.Transport {
    [TestClass]
    public class EncryptionTests {
        private RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();

        [TestMethod]
        public void TestRijndael() {
            byte[] key = new byte[(int)SyncIOKeySize.Bit128];
            RNG.GetNonZeroBytes(key);
           var enc = new SyncIOEncryptionRijndael(key);

            var pack = "SyncIO";
            var encrypt = enc.Encrypt(Encoding.UTF8.GetBytes(pack));
            var decrypt = Encoding.UTF8.GetString(enc.Decrypt(encrypt));
            Assert.AreEqual(pack, decrypt);
        }

        [TestMethod]
        public void TestRSA() {
            byte[] key = new byte[(int)SyncIOKeySize.Bit128];
            RNG.GetNonZeroBytes(key);
            var enc = new SyncIOEncryptionRSA();

            var pack = "SyncIO";

            var encrypt = enc.Encrypt(Encoding.UTF8.GetBytes(pack));
            var decrypt = Encoding.UTF8.GetString(enc.Decrypt(encrypt));
            Assert.AreEqual(pack, decrypt);

            var secEnc = new SyncIOEncryptionRSA(enc.PublicKey);
            encrypt = secEnc.Encrypt(Encoding.UTF8.GetBytes(pack));
            decrypt = Encoding.UTF8.GetString(enc.Decrypt(encrypt));

            Assert.AreEqual(pack, decrypt);
        }

        internal enum SyncIOKeySize {
            Bit128 = 16,
            Bit192 = 24,
            Bit256 = 32
        }

    }
}
