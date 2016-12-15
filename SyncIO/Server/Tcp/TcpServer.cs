using SyncIO.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Server.Tcp {
    internal class TcpServer {
        public TransportProtocal Protocal { get; }
        public Socket NetworkSocket { get; private set; }
        public bool Connected => (NetworkSocket?.Connected ?? false) && SuccessfulConnection;

        private bool SuccessfulConnection = false;

        public TcpServer(TransportProtocal _protocal) {
            Protocal = _protocal;
        }

        public TcpServer() :this(TransportProtocal.IPv4) {
        }

        /// <summary>
        /// Disposes old socket if exists. 
        /// Creates a new TCP socket with either IPv4 or IPv6 depending on what is specified in the constructor.
        /// </summary>
        private void CreateNewSocket() {
            NetworkSocket?.Dispose();
            if (Protocal == TransportProtocal.IPv6)
                NetworkSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            else
                NetworkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Connect(string host, int port) {
            CreateNewSocket();
            try {
                NetworkSocket.Connect(host, port);
                SuccessfulConnection = true;
            }
            catch {
                SuccessfulConnection = false;
            }
            return SuccessfulConnection;
        }

        public bool Connect(EndPoint endpoint) {
            CreateNewSocket();
            try {
                SuccessfulConnection = true;
            }
            catch {
                SuccessfulConnection = false;
            }
            return SuccessfulConnection;
        }
    }
}
