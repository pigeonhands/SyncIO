namespace SyncIO.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using SyncIO.Client;
    using SyncIO.Network;
    using SyncIO.Server;
    using SyncIO.Server.RemoteCalls;
    using SyncIO.Transport.RemoteCalls;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SyncIORemoteCalls
    {
        [TestMethod]
        public void TestRemoteCalls()
        {
            var client = new SyncIOClient();
            var server = new SyncIOServer();

            var Test1 = server.RegisterRemoteFunction("Test1", new Func<string, int>((string string1) => string1.Length));
            server.RegisterRemoteFunction("Test2", new Func<string, string>((string string1) => string.Concat(string1.Reverse())));
            server.RegisterRemoteFunction("Test3", new Func<string, char>((string string1) => string1.FirstOrDefault()));

            Test1.SetAuthFunc((SyncIOConnectedClient requester, RemoteFunctionBind callingFunc) =>
            { //Example of authenticating call to Test1
                return true; //Always allow call
            });

            var listenSock = server.ListenTCP(6000);
            Assert.IsNotNull(listenSock);

            client.OnDisconnect += (sCl, err) =>
            {
                throw new AssertFailedException(err.Message, err);
            };

            Assert.IsTrue(client.Connect("127.0.0.1", 6000));

            Assert.IsTrue(client.WaitForHandshake());

            var testParam = "Hello World";
            var func1 = client.GetRemoteFunction<int>("Test1");
            var func2 = client.GetRemoteFunction<string>("Test2");
            var func3 = client.GetRemoteFunction<char>("Test3");

            Assert.AreEqual(testParam.Length, func1.CallWait(testParam));
            Assert.AreEqual(string.Concat(testParam.Reverse()), func2.CallWait(testParam));
            Assert.AreEqual(testParam.FirstOrDefault(), func3.CallWait(testParam));
            func1.CallWait(1, 2, 3, 4, 5);
            Assert.AreEqual(func1.LastStatus, FunctionResponceStatus.InvalidParameters);
        }

    }
}