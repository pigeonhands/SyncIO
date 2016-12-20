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


        private Action<SyncIOConnectedClient, object[]> ArrayHandler;
        private Action<SyncIOConnectedClient, IPacket> GenericHandler;
        private Dictionary<Type, PacketCallback<InternalSyncIOConnectedClient>> PacketCallbacks = new Dictionary<Type, PacketCallback<InternalSyncIOConnectedClient>>();
        private object CallbackLock = new object();

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

        private bool RaisePacketHandler(Type packetType, InternalSyncIOConnectedClient client, IPacket data) {
            PacketCallback<InternalSyncIOConnectedClient> callback = null;
            lock (CallbackLock) {
                if (PacketCallbacks.ContainsKey(packetType))
                    callback = PacketCallbacks[packetType];
            }
            if (callback == null) {
                return false;
            } else {
                callback?.Raise(client, data);
                return true;
            }
        }

        private void ReceveHandler(InternalSyncIOConnectedClient client, IPacket data) {
          
            Type packetType = data.GetType();

            if(packetType == typeof(ObjectArrayPacket)) {
                ArrayHandler?.Invoke(client, ((ObjectArrayPacket)data).Data);
            }else {
                if (!RaisePacketHandler(packetType, client, data))
                    GenericHandler?.Invoke(client, data);
            }
        }

        /// <summary>
        /// Add handler for raw object array receve
        /// </summary>
        /// <param name="callback"></param>
        public void AddHandler(Action<SyncIOConnectedClient, object[]> callback) {
            lock (CallbackLock) {
                ArrayHandler = callback;
            }
        }

        /// <summary>
        /// Add handler for IPacket type receve
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public void AddHandler<T>(Action<SyncIOConnectedClient, T> callback) where T :class, IPacket {
            var cb = PacketCallback<SyncIOConnectedClient>.Create<InternalSyncIOConnectedClient, T>(callback);
            lock (CallbackLock) {
                if (PacketCallbacks.ContainsKey(cb.Type))
                    PacketCallbacks[cb.Type] = cb;
                else
                    PacketCallbacks.Add(cb.Type, cb);
            }
        }

        /// <summary>
        /// Add handler for all IPacket packets.
        /// If another handler is raised for the type of IPacket, this callback will not be called for it.
        /// </summary>
        /// <param name="callback"></param>
        public void AddHandler(Action<SyncIOConnectedClient, IPacket> callback) {
            lock (CallbackLock) {
                GenericHandler = callback;
            }
        }

    }
}
