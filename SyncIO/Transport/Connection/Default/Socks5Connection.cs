using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.Connection.Default {
    public class Socks5Connection : IConnectionMethod {

        public bool ConnectionEstablished { get; private set; }

        private Socket NetworkSocket;
        private IPEndPoint socksAddress;

        public Socks5Connection(IPEndPoint proxyServerEndPoint) {
            socksAddress = proxyServerEndPoint;
        }

        private bool AttempSocketConnection(string ip, int port) {
            NetworkSocket?.Dispose();
            NetworkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                NetworkSocket.Connect(socksAddress);
                return true;
            }catch{
                return false;
            }
        }

        public Socket GetSocket() {
            if (NetworkSocket == null)
                throw new Exception("Socks5Connection has not made a connection attempt yet.");

            return NetworkSocket;
        }

        public SocksConnectionResponce Connect(string host, int port) {

            if (!AttempSocketConnection(host, port))
                return SocksConnectionResponce.NoConnection;

            var recBuffer = new byte[512];
            var packet = new byte[] {
                0x5, // Socks protocal version
                2,   //number of methods
                (byte)SocksAuthenticationMethod.NoAuth,
                (byte)SocksAuthenticationMethod.UserPass
            };

            NetworkSocket.Send(packet);

            if (NetworkSocket.Receive(recBuffer, 2, SocketFlags.None) != 2)
                return SocksConnectionResponce.InvalidDataRetrieve;

            if (recBuffer[0] != 0x5)
                return SocksConnectionResponce.InvalidDataRetrieve;

            var authMethod = (SocksAuthenticationMethod)recBuffer[1];

            if(authMethod != SocksAuthenticationMethod.NoAuth) {
                NetworkSocket.Dispose();
                NetworkSocket = null;
                return SocksConnectionResponce.InvalidAuthentication;
            }



            return SocksConnectionResponce.InvalidAuthentication;
        }

       
    }

    public enum SocksConnectionResponce {
        Success,
        NoConnection,
        InvalidAuthentication,
        InvalidDataRetrieve
    }

    internal enum SocksAuthenticationMethod : byte {
        NoAuth = 0,
        UserPass = 2,
        NONE=0xff
    }
}
