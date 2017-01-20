using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncIO.Client;
using SyncIO.Network;
using SyncIO.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncIOTests {
    [TestClass]
    public class SyncIOProtocal {

        [TestMethod]
        public void TestUDP() {
            var packets = 10;

            var client = new SyncIOClient();
            var server = new SyncIOServer();

           
            object[] sent = new object[] { 1, 2, 3, "hello", "world" };
            int receved = 0;

            server.SetHandler((SyncIOConnectedClient s, object[] data) => {
                Assert.AreEqual(client.ID, s.ID);
                CollectionAssert.AreEqual(sent, data);
                receved++;
            });

            var sock = server.ListenTCP(55455);
            Assert.IsNotNull(sock);

            sock.TryOpenUDPConnection();
            Assert.IsTrue(sock.HasUDP);

            Assert.IsTrue(client.Connect("127.0.0.1", 55455));
            Assert.IsTrue(client.WaitForHandshake());
            client.TryOpenUDPConnection();
            Assert.IsTrue(client.WaitForUDP());

            for (int i = 0; i < packets; i++)
                client.SendUDP(sent);
            Thread.Sleep(3000); //Wait 3 secodns

            Assert.AreEqual(packets, receved);
        }


    }
}
