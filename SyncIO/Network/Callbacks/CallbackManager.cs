namespace SyncIO.Network.Callbacks
{
    using System;
    using System.Collections.Generic;

    using SyncIO.Transport.Packets;
    using SyncIO.Transport.Packets.Internal;

    /// <summary>
    /// Threadsafe callback handler.
    /// </summary>
    public class CallbackManager<ClientType> where ClientType : ISyncIOClient
    {
        private Action<ClientType, object[]> _arrayHandler;
        private Action<ClientType, IPacket> _genericHandler;
        private readonly Dictionary<Type, PacketCallback<ClientType>> _packetCallbacks;
        private readonly object _callbackLock;

        public CallbackManager()
        {
            _packetCallbacks = new Dictionary<Type, PacketCallback<ClientType>>();
            _callbackLock = new object();
        }

        /// <summary>
        /// Add handler for raw object array receive
        /// </summary>
        /// <param name="callback"></param>
        public void SetArrayHandler(Action<ClientType, object[]> callback)
        {
            lock (_callbackLock)
            {
                _arrayHandler = callback;
            }
        }

        /// <summary>
        /// Removes the handler for the specified type
        /// </summary>
        /// <typeparam name="T">type to remove the handler</typeparam>
        public void RemoveHandler<T>()
        {
            lock (_callbackLock)
            {
                var fType = typeof(T);
                if (_packetCallbacks.ContainsKey(fType))
                {
                    _packetCallbacks.Remove(fType);
                }
            }
        }

        /// <summary>
        /// Add handler for IPacket type receive
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public void SetHandler<T>(Action<ClientType, T> callback) where T : class, IPacket
        {
            var cb = PacketCallback<ClientType>.Create<T>(callback);
            lock (_callbackLock)
            {
                if (_packetCallbacks.ContainsKey(cb.Type))
                    _packetCallbacks[cb.Type] = cb;
                else
                    _packetCallbacks.Add(cb.Type, cb);
            }
        }

        /// <summary>
        /// Add handler for all IPacket packets.
        /// If another handler is raised for the type of IPacket, this callback will not be called for it.
        /// </summary>
        /// <param name="callback"></param>
        public void SetPacketHandler(Action<ClientType, IPacket> callback)
        {
            lock (_callbackLock)
            {
                _genericHandler = callback;
            }
        }

        private bool RaisePacketHandler(Type packetType, ClientType client, IPacket data)
        {
            PacketCallback<ClientType> callback = null;
            lock (_callbackLock)
            {
                if (_packetCallbacks.ContainsKey(packetType))
                {
                    callback = _packetCallbacks[packetType];
                }
            }

            if (callback == null)
            {
                return false;
            }

            callback?.Raise(client, data);
            return true;
        }

        public void Handle(ClientType client, IPacket data)
        {
            var packetType = data.GetType();
            if (packetType == typeof(ObjectArrayPacket))
            {
                _arrayHandler?.Invoke(client, ((ObjectArrayPacket)data).Data);
            }
            else
            {
                if (!RaisePacketHandler(packetType, client, data))
                {
                    _genericHandler?.Invoke(client, data);
                }
            }
        }
    }
}