using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncIO.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SyncIOTests.Transport {
    [TestClass]
    public class PacketDefragmenterTests {

        [TestMethod]
        public void TestDefragmentation() {
            var RNG = new RNGCryptoServiceProvider();
            for(int bsize = 4; bsize < 50; bsize++) { //Use diffrent buffer sizes
                var pd = new PacketDefragmenter(bsize);

                for (int i = 4; i < 100; i++) {//checking that PacketDefragmenter works past the first packet and with diffrent size data
                    byte[] receved = null;
                    var data = new byte[i];
                    RNG.GetNonZeroBytes(data);
                    var packet = BitConverter.GetBytes(data.Length).Concat(data).ToArray();

                    using (var networkSimulator = new MemoryStream(packet)) {

                        while (receved == null) {
                            receved = pd.Process(networkSimulator.Read(pd.ReceveBuffer, pd.BufferIndex, pd.BytesToReceve));
                        }
                    }
                    CollectionAssert.AreEqual(data, receved);
                }
            }
            
        }
    }
}
