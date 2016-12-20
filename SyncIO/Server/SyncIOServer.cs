using SyncIO.Network;
using SyncIO.Network.Callbacks;
using SyncIO.Server.Network;
using SyncIO.Transport;
using SyncIO.Transport.Packets;
using SyncIO.Transport.Packets.Internal;
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
        private CallbackManager Callbacks;


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
            client.BeginReceve(ReceveHandler);
            OnClientConnect?.Invoke(this, client);
        }

        

        private void ReceveHandler(InternalSyncIOConnectedClient client, IPacket data) {
            Callbacks.Handle(client, data);
        }


        /// <summary>
        /// Add handler for raw object array receve
        /// </summary>
        /// <param name="callback"></param>
        public void AddHandler(Action<ISyncIOClient, object[]> callback) {
            Callbacks.AddArrayHandler(callback);
        }

        /// <summary>
        /// Add handler for IPacket type receve
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public void AddHandler<T>(Action<ISyncIOClient, T> callback) where T : class, IPacket {
            Callbacks.AddHandler<T>(callback);
        }

        /// <summary>
        /// Add handler for all IPacket packets.
        /// If another handler is raised for the type of IPacket, this callback will not be called for it.
        /// </summary>
        /// <param name="callback"></param>
        public void AddHandler(Action<ISyncIOClient, IPacket> callback) {
            Callbacks.AddPacketHandler(callback);
        }

    }
}
