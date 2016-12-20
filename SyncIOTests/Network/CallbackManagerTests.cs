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

            var callbacks = new CallbackManager<ISyncIOClient>();


            callbacks.SetArrayHandler((ISyncIOClient client, object[] d) => {
                CollectionAssert.AreEqual(d, objectTest);
            });

            callbacks.SetHandler<Test1>((ISyncIOClient client, Test1 t) => {
                Assert.AreEqual(TestClass1, t);
            });

            callbacks.Handle(null, new ObjectArrayPacket(objectTest));

            callbacks.SetPacketHandler((ISyncIOClient client, IPacket p) => {
                Assert.IsInstanceOfType(p, typeof(Test2));
            });

            callbacks.Handle(null, new Test2());

            callbacks.SetPacketHandler((ISyncIOClient client, IPacket p) => {
                Assert.IsNotInstanceOfType(p, typeof(Test2));
            });

            callbacks.Handle(null, new Test3());
        }

    }
}
