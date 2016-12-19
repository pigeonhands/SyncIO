using SyncIO.Server.Network;
using SyncIO.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Server{
    public delegate void OnClientConnectDelegate(SyncIOServer sender, SyncIOConnectedClient client);
    public class SyncIOServer{
        public event OnClientConnectDelegate OnClientConnect;
        public TransportProtocal Protocal { get; }

        private Packager Packager;
        public SyncIOServer(TransportProtocal _protocal, Packager _packager) {
            Protocal = _protocal;
            Packager = _packager;
        }

        public SyncIOServer() : this(TransportProtocal.IPv4, new Packager()) {
        }

        /// <summary>
        /// Listens on a new port.
        /// </summary>
        /// <param name="port">Port to listen</param>
        /// <returns>The open socket on success, else null.</returns>
        public SyncIOSocket ListenTCP(int port) {
            var tcpSock = new TcpSocket(Protocal);
            tcpSock.OnClientConnect += TcpSock_OnClientConnect;
            if (!tcpSock.BeginAccept(port))
                return null;
            return tcpSock;
        }

        private void TcpSock_OnClientConnect(TcpSocket sender, Socket s) {
            var client = new InternalSyncIOConnectedClient(s, Packager);
            
            client.Receve();
        }
    }
}
