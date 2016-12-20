using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncIO.Transport;
using SyncIO.Transport.Encryption.Defaults;
using SyncIO.Transport.Packets.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIOTests.Transport {
    [TestClass]
    public class PackagerTesting {

        [TestMethod]
        public void TestPackager() {
            var packer = new Packager();



            var objArra = new object[] { "SyncIO", 1234, 55f };

            var packedArr = packer.PackArray(objArra, null);
            var unpackedArr = packer.Unpack(packedArr) as ObjectArrayPacket;
            CollectionAssert.AreEqual(unpackedArr.Data, objArra);

            var arrObj = new ObjectArrayPacket(objArra);
            var packedObj = packer.Pack(arrObj);
            var unpackedObj = packer.Unpack(packedObj) as ObjectArrayPacket;
            CollectionAssert.AreEqual(unpackedObj.Data, arrObj.Data);

            //Encryption
            var config = new PackConfig();
            config.Encryption = PackConfig.GenerateNewEncryption(SyncIOKeySize.Bit128);

            packedArr = packer.PackArray(objArra, config);
            unpackedArr = packer.Unpack(packedArr, config) as ObjectArrayPacket;
            CollectionAssert.AreEqual(unpackedArr.Data, objArra);

             arrObj = new ObjectArrayPacket(objArra);
             packedObj = packer.Pack(arrObj, config);
             unpackedObj = packer.Unpack(packedObj, config) as ObjectArrayPacket;
            CollectionAssert.AreEqual(unpackedObj.Data, arrObj.Data);

           //RSA

            var enc = new SyncIOEncryptionRSA();
            config.Encryption = enc;

            var encrypt = packer.PackArray(objArra, config);
            var decrypt = (packer.Unpack(encrypt, config) as ObjectArrayPacket).Data;
            CollectionAssert.AreEqual(objArra, decrypt);

            config.Encryption = new SyncIOEncryptionRSA(enc.PublicKey);
            encrypt = packer.PackArray(objArra, config);
            config.Encryption = enc;
            decrypt = (packer.Unpack(encrypt, config) as ObjectArrayPacket).Data;

            CollectionAssert.AreEqual(objArra, decrypt);
                      
        }

    }
}
