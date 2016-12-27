using SyncIO.Network;
using SyncIO.Network.Callbacks;
using SyncIO.Server;
using SyncIO.Transport;
using SyncIO.Transport.Packets;
using SyncIO.Transport.Packets.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace SyncIO.Server{
    public delegate void OnClientConnectDelegate(SyncIOServer sender, SyncIOConnectedClient client);
    public class SyncIOServer : IEnumerable<SyncIOSocket>{
        public event OnClientConnectDelegate OnClientConnect;
        public TransportProtocal Protocal { get; }

        private Packager Packager;
        private CallbackManager<SyncIOConnectedClient> Callbacks;
        private Func<Guid> GuidGenerator = () => Guid.NewGuid();
        private List<SyncIOSocket> OpenSockets = new List<SyncIOSocket>();

        public SyncIOServer(TransportProtocal _protocal, Packager _packager) {
            Protocal = _protocal;
            Packager = _packager;
            Callbacks = new CallbackManager<SyncIOConnectedClient>();
        }

        public SyncIOServer() : this(TransportProtocal.IPv4, new Packager()) {
        }

        /// <summary>
        /// Listens on a new port.
        /// </summary>
        /// <param name="port">Port to listen</param>
        /// <returns>The open socket on success, else null.</returns>
        public SyncIOSocket ListenTCP(int port) {
            var tcpSock = new TcpServerSocket(Protocal);
            tcpSock.OnClientConnect += TcpSock_OnClientConnect;
            if (!tcpSock.BeginAccept(port))
                return null;

            OpenSockets.Add(tcpSock);
            tcpSock.OnClose += (s) => {
                OpenSockets.Remove(s);
            };
           
            return tcpSock;
        }

        private void TcpSock_OnClientConnect(TcpServerSocket sender, Socket s) {
            var client = new InternalSyncIOConnectedClient(s, Packager);
            client.SetID(GuidGenerator());
            client.BeginReceve(ReceveHandler);
            client.Send((cl) => {
                OnClientConnect?.Invoke(this, cl);//Trigger event after handshake packet has been sent.
            }, new HandshakePacket(true, client.ID));
        }

        private void ReceveHandler(InternalSyncIOConnectedClient client, IPacket data) {
            Callbacks.Handle(client, data);
        }

        /// <summary>
        /// If not set, clients may receve duplicate Guids.
        /// </summary>
        /// <param name="_call">Call to guid generator. By default is Guid.NewGuid</param>
        public void SetGuidGenerator(Func<Guid> _call) {
            if (_call == null)
                return;
            GuidGenerator = _call;
        }


        /// <summary>
        /// Add handler for raw object array receve
        /// </summary>
        /// <param name="callback"></param>
        public void SetHandler(Action<SyncIOConnectedClient, object[]> callback) {
            Callbacks.SetArrayHandler(callback);
        }

        /// <summary>
        /// Add handler for IPacket type receve
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public void SetHandler<T>(Action<SyncIOConnectedClient, T> callback) where T : class, IPacket {
            Callbacks.SetHandler<T>(callback);
        }

        /// <summary>
        /// Add handler for all IPacket packets.
        /// If another handler is raised for the type of IPacket, this callback will not be called for it.
        /// </summary>
        /// <param name="callback"></param>
        public void SetHandler(Action<SyncIOConnectedClient, IPacket> callback) {
            Callbacks.SetPacketHandler(callback);
        }

        public IEnumerator<SyncIOSocket> GetEnumerator() {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return OpenSockets.GetEnumerator();
        }
    }
}