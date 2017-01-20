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
            var done = new ManualResetEvent(false);

            var client = new SyncIOClient();
            var server = new SyncIOServer();

            Guid clientID = Guid.Empty;
            object[] sent = new object[] { 1, 2, 3, "hello", "world" };
            object[] receved = new object[0];

            server.SetHandler((SyncIOConnectedClient s, object[] data) => {
                clientID = s.ID;
                receved = data;
                done.Set();
            });

            var sock = server.ListenTCP(55455);
            Assert.IsNotNull(sock);

            sock.TryOpenUDPConnection();
            Assert.IsTrue(sock.HasUDP);

            Assert.IsTrue(client.Connect("127.0.0.1", 55455));
            Assert.IsTrue(client.WaitForHandshake());
            client.TryOpenUDPConnection();
            Assert.IsTrue(client.WaitForUDP());

           client.SendUDP(sent);
            done.WaitOne();

            Assert.AreEqual(client.ID, clientID);
            CollectionAssert.AreEqual(sent, receved);
        }


    }
}
