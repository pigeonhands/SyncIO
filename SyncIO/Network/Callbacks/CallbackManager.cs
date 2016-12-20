using SyncIO.Server;
using SyncIO.Transport.Packets;
using SyncIO.Transport.Packets.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Network.Callbacks {

    /// <summary>
    /// Threadsafe callback handler.
    /// </summary>
    internal class CallbackManager<ClientType> where ClientType : ISyncIOClient {

        private Action<ClientType, object[]> ArrayHandler;
        private Action<ClientType, IPacket> GenericHandler;
        private Dictionary<Type, PacketCallback<ClientType>> PacketCallbacks = new Dictionary<Type, PacketCallback<ClientType>>();
        private object CallbackLock = new object();

        /// <summary>
        /// Add handler for raw object array receve
        /// </summary>
        /// <param name="callback"></param>
        public void SetArrayHandler(Action<ClientType, object[]> callback) {
            lock (CallbackLock) {
                ArrayHandler = callback;
            }
        }

        /// <summary>
        /// Add handler for IPacket type receve
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public void SetHandler<T>(Action<ClientType, T> callback) where T : class, IPacket {
            var cb = PacketCallback<ClientType>.Create<T>(callback);
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
        public void SetPacketHandler(Action<ClientType, IPacket> callback) {
            lock (CallbackLock) {
                GenericHandler = callback;
            }
        }

        private bool RaisePacketHandler(Type packetType, ClientType client, IPacket data) {
            PacketCallback<ClientType> callback = null;
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

        public void Handle(ClientType client, IPacket data) {
            Type packetType = data.GetType();

            if (packetType == typeof(ObjectArrayPacket)) {
                ArrayHandler?.Invoke(client, ((ObjectArrayPacket)data).Data);
            } else {
                if (!RaisePacketHandler(packetType, client, data))
                    GenericHandler?.Invoke(client, data);
            }
        }
    }
}
