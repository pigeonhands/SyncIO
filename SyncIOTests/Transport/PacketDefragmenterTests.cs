using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncIO.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIOTests.Transport {
    [TestClass]
    public class PacketDefragmenterTests {

        [TestMethod]
        public void TestDefragmentation() {

            for(int bsize = 4; bsize < 50; bsize++) { //Use diffrent buffer sizes
                var pd = new PacketDefragmenter(bsize);

                for (int i = 0; i < 1000; i++) {//checking that PacketDefragmenter works past the first packet
                    byte[] receved = null;
                    var data = "SyncIO packet defragmentation test";
                    var strBytes = Encoding.UTF8.GetBytes(data);
                    var packet = BitConverter.GetBytes(strBytes.Length).Concat(strBytes).ToArray();

                    using (var networkSimulator = new MemoryStream(packet)) {

                        while (receved == null) {
                            receved = pd.Process(networkSimulator.Read(pd.ReceveBuffer, pd.BufferIndex, pd.BytesToReceve));
                        }
                    }
                    var retStr = Encoding.UTF8.GetString(receved);
                    Assert.AreEqual(retStr, data);
                }
            }
            
        }
    }
}
