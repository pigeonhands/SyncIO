using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncIO.Network;
using SyncIO.Network.Callbacks;
using SyncIO.Transport;
using SyncIO.Transport.Packets;
using SyncIO.Transport.Packets.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIOTests.Network {
    [TestClass]
    public class CallbackManagerTests {

        

        [TestMethod]
        public void TestCallbacks() {
            var objectTest = new object[] {
                "Test",
                1,
                22f,
                55.7m,
                new int[] {1,2,3,4,5}
            };

            var TestClass1 = new Test1(55);

            var callbacks = new CallbackManager<testClient>();

            var Client1 = new testClient();
            var Client2 = new testClient();


            callbacks.SetArrayHandler((testClient client, object[] d) => {
                Assert.AreEqual(client, Client1);
                CollectionAssert.AreEqual(d, objectTest);
            });

            callbacks.SetHandler<Test1>((testClient client, Test1 t) => {
                Assert.AreEqual(client, Client2);
                Assert.AreEqual(TestClass1, t);
            });

            callbacks.Handle(Client1, new ObjectArrayPacket(objectTest));
            callbacks.Handle(Client2, TestClass1);

            callbacks.SetPacketHandler((testClient client, IPacket p) => {
                Assert.AreEqual(client, Client1);
                Assert.IsInstanceOfType(p, typeof(Test2));
            });

            callbacks.Handle(Client1, new Test2());

            callbacks.SetPacketHandler((testClient client, IPacket p) => {
                Assert.AreNotEqual(client, Client1);
                Assert.IsNotInstanceOfType(p, typeof(Test2));
            });

            callbacks.Handle(Client2, new Test3());
        }

        public class testClient : ISyncIOClient {
            public void Send(IPacket packet) {
            }

            public void Send(params object[] data) {
            }
        }

        public class Test1 : IPacket {
            public int Data { get; set; }
            public Test1(int _d) {
                Data = _d;
            }
        }

        public class Test2 : IPacket {
        }
        public class Test3 : IPacket {
        }

    }

}
