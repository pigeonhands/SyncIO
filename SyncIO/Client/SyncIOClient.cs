using SyncIO.Network;
using SyncIO.Network.Callbacks;
using SyncIO.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SyncIO.Transport.Packets;
using SyncIO.Transport.Packets.Internal;
using System.Threading;

namespace SyncIO.Client {

    public delegate void OnHandshakeDelegate(SyncIOClient sender, Guid id, bool success);
    public delegate void OnDisconnectDelegate(SyncIOClient sender, Exception ex);
    public class SyncIOClient : SyncIOSocket, ISyncIOClient {

        public event OnHandshakeDelegate OnHandshake;
        public event OnDisconnectDelegate OnDisconnect;

        /// <summary>
        /// Id of client supplied by server.
        /// </summary>
        public Guid ID => Connection?.ID ?? Guid.Empty;
        public TransportProtocal Protocal { get; }
        public bool Connected => ID != Guid.Empty;

        private CallbackManager<SyncIOClient> Callbacks;
        private InternalSyncIOConnectedClient Connection;
        private Packager Packager;
        private bool HandshakeComplete;
        private ManualResetEvent HandshakeEvent = new ManualResetEvent(false);

        public SyncIOClient(TransportProtocal _protocal, Packager _packager) {
            Protocal = _protocal;
            Packager = _packager;
            Callbacks = new CallbackManager<SyncIOClient>();

            Callbacks.SetHandler<HandshakePacket>((c, p) => {
                HandshakeComplete = p.Success;
                Connection.SetID(p.ID);
                HandshakeEvent?.Set();
                OnHandshake?.Invoke(this, ID, HandshakeComplete);
            });
        }

        public SyncIOClient(): this(TransportProtocal.IPv4, new Packager()) {
        }

        /// <summary>
        /// Possably add support for connecting to multiple servers.
        /// </summary>
        private Socket NewSocket() {
            if (Protocal == TransportProtocal.IPv6)
                return new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            else
                return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void SetupConnection(Socket s) {
            SetTcpKeepAlive(s);
            EndPoint = ((IPEndPoint)s.RemoteEndPoint);
            Connection = new InternalSyncIOConnectedClient(s, Packager);
            Connection.BeginReceve(ReceveHandler);
            Connection.OnDisconnect += Connection_OnDisconnect;
        }

        private void Connection_OnDisconnect(SyncIOConnectedClient client, Exception ex) {
            OnDisconnect?.Invoke(this, ex);
        }

        private void ReceveHandler(InternalSyncIOConnectedClient client, IPacket data) {
            Callbacks.Handle(this, data);
        }

        /// <summary>
        /// Establishes a TCP connection to a SyncIOServer.
        /// Sending will fail untill handshake is completed.
        /// Bind to OnHandshake event for notify.
        /// </summary>
        /// <param name="host">IP address</param>
        /// <param name="port">Port</param>
        /// <returns></returns>
        public bool Connect(string host, int port) {
            var sock = NewSocket();
            try {
                sock.Connect(host, port);
                SetupConnection(sock);
                return true;
            } catch {
                Connection = null;
                return false;
            }
        }

        /// <summary>
        /// Add handler for raw object array receve
        /// </summary>
        /// <param name="callback"></param>
        public void SetHandler(Action<SyncIOClient, object[]> callback) {
            Callbacks.SetArrayHandler(callback);
        }

        /// <summary>
        /// Add handler for IPacket type receve
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public void SetHandler<T>(Action<SyncIOClient, T> callback) where T : class, IPacket {
            Callbacks.SetHandler<T>(callback);
        }

        /// <summary>
        /// Add handler for all IPacket packets.
        /// If another handler is raised for the type of IPacket, this callback will not be called for it.
        /// </summary>
        /// <param name="callback"></param>
        public void SetHandler(Action<SyncIOClient, IPacket> callback) {
            Callbacks.SetPacketHandler(callback);
        }

        public void Send(Action<SyncIOConnectedClient> afterSend, params object[] data) {
            if (Connected) {
                Connection.Send(afterSend, data);
            }
        }

        public void Send(Action<SyncIOConnectedClient> afterSend, IPacket packet) {
            if (Connected) {
                Connection.Send(afterSend, packet);
            }
        }

        public void Send(params object[] data) {
            Send(null, data);
        }

        public void Send(IPacket packet) {
            Send(null, packet);
        }

        /// <summary>
        /// Blocks and waits for handshake.
        /// </summary>
        /// <returns></returns>
        public bool WaitForHandshake() {
            if (Connected)
                return true;

            HandshakeEvent?.WaitOne();
            HandshakeEvent?.Dispose();
            HandshakeEvent = null;
            return Connected;
        }
    }

}
