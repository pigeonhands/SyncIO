using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncIO.Client;
using SyncIO.Network;
using SyncIO.Server;
using SyncIO.Transport;
using SyncIO.Transport.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncIOTests {
    [TestClass]
    public class SyncIONetwork {

        [Serializable]
        class CustomDataSending : IPacket {
            public int Num { get; set; }
            public string str { get; set; }
            public float fl { get; set; }
        }

        [TestMethod]
        public void TestPacketSending(){

            var done = new ManualResetEvent(false);

            var packer = new Packager(new Type[] {
                typeof(CustomDataSending)
                });

            var arrPayload = new object[] {
                9,
                "SyncIOTest",
                "Example",
                666.333f
            };

            var cPayload = new CustomDataSending() {
                Num = 5555,
                str = "SyncIOTest",
                fl = 6666.4444f
            };

            var server = new SyncIOServer(TransportProtocal.IPv4, packer);
            
            Guid cID = Guid.NewGuid();

            server.OnClientConnect += (s, c) => {
                Console.WriteLine("Connected.");
                cID = c.ID;
                c.OnDisconnect += (rtcl, err) => {
                    throw err;
                };
            };

            server.SetHandler((SyncIOConnectedClient s, object[] dat) => {
                CollectionAssert.AreEqual(arrPayload, dat);
                s.Send(cPayload);
            });

            Assert.IsNotNull(server.ListenTCP(1234));

            var client = new SyncIOClient(TransportProtocal.IPv4, packer);

            client.OnDisconnect += (sCl, err) => {
                throw err;
            };

            client.SetHandler<CustomDataSending>((s, d) => {
                Assert.AreEqual(cPayload.fl, d.fl);
                Assert.AreEqual(cPayload.Num, d.Num);
                Assert.AreEqual(cPayload.str, d.str);
                done.Set();
            });

            client.OnHandshake += (s, id, succ) => {
                Assert.IsTrue(succ);
                Thread.Sleep(100); //Wait for server callback to finish
                Assert.AreEqual(cID, id);
                s.Send(arrPayload);
            };

            Assert.IsTrue(client.Connect("127.0.0.1", 1234));
           

            Assert.IsTrue(done.WaitOne(10 * 1000));
        }

    }
}
