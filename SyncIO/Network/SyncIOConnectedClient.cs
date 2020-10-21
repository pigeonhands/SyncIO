namespace SyncIO.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using SyncIO.Network.Callbacks;
    using SyncIO.Transport;
    using SyncIO.Transport.Compression;
    using SyncIO.Transport.Encryption;
    using SyncIO.Transport.Packets;

    public delegate void OnClientDisconnectDelegate(SyncIOConnectedClient client, Exception ex);

    /// <summary>
    /// A client that is connected to a SyncIOServer
    /// Used from receving/sending from the SyncIOServer.
    /// </summary>
    public abstract class SyncIOConnectedClient : ISyncIOClient
    {
        public event OnClientDisconnectDelegate OnDisconnect;
        /// <summary>
        /// Id of connected client.
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// General tag data not used internaly by SyncIO.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Remote EndPoint of client
        /// </summary>
        public IPEndPoint EndPoint => (IPEndPoint)NetworkSocket?.RemoteEndPoint;

        /// <summary>
        /// All client file transfers
        /// </summary>
        public IDictionary<Guid, TransferQueue> Transfers { get; } = new Dictionary<Guid, TransferQueue>();

        /// <summary>
        /// Underlying socket connection for the client
        /// </summary>
        protected Socket NetworkSocket { get; set; }

        /// <summary>
        /// Packet packaging configuration i.e. set encryption, compression options, etc
        /// </summary>
        internal PackConfig PackagingConfiguration { get; set; }


        public void Send(params object[] data)
        {
            Send(null, data);
        }

        public void Send(IPacket packet)
        {
            Send(null, packet);
        }

        public virtual void Send(Action<SyncIOConnectedClient> afterSend, params object[] data)
        {
        }

        public virtual void Send(Action<SyncIOConnectedClient> afterSend, IPacket packet)
        {
        }

        public void Disconnect(Exception ex)
        {
            if (NetworkSocket != null)
            {
                NetworkSocket.Shutdown(SocketShutdown.Both);
                NetworkSocket.Dispose();
                NetworkSocket = null;
                OnDisconnect?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// Sets the encryption for incoming and outgoing network traffic.
        /// </summary>
        /// <param name="encryption">Encryption type to use.</param>
        public void SetEncryption(ISyncIOEncryption encryption)
        {
            if (PackagingConfiguration == null)
            {
                PackagingConfiguration = new PackConfig();
            }
            PackagingConfiguration.Encryption = encryption;
        }

        /// <summary>
        /// Sets the compression for incoming and outgoing network traffic.
        /// Warning: Enabling compression will make smaller packets slightly
        /// bigger, although larger packets will reduce in size.
        /// </summary>
        /// <param name="compression">Compress type to use.</param>
        public void SetCompression(ISyncIOCompression compression)
        {
            if (PackagingConfiguration == null)
            {
                PackagingConfiguration = new PackConfig();
            }
            PackagingConfiguration.Compression = compression;
        }
    }

    /// <summary>
    /// Used to create SyncIOConnectedClient objects. 
    /// </summary>
    internal class InternalSyncIOConnectedClient : SyncIOConnectedClient
    {
        #region Variables

        /// <summary>
        /// Multithread sync object
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// Send queue for client
        /// </summary>
        private readonly Queue<QueuedPacket> _sendQueue = new Queue<QueuedPacket>();

        /// <summary>
        /// Used to handle receving of data.
        /// </summary>
        private readonly PacketDefragmenter _defragger;

        private Action<InternalSyncIOConnectedClient, IPacket> _receiveCallback;

        #endregion

        #region Properties

        public Packager Packager { get; }

        #endregion

        #region Constructor(s)

        public InternalSyncIOConnectedClient(Socket s, Packager p) 
            : this(s, p, 1024 * 5)
        {
        }

        public InternalSyncIOConnectedClient(Socket s, Packager p, int bufferSize)
        {
            NetworkSocket = s ?? throw new Exception("Socket not valid.");
            Packager = p;
            _defragger = new PacketDefragmenter(bufferSize);
        }

        #endregion

        #region Public Methods

        public override void Send(Action<SyncIOConnectedClient> afterSend, params object[] data)
        {
            var rawData = Packager.PackArray(data, PackagingConfiguration);
            HandleRawBytes(rawData, afterSend);
        }

        public override void Send(Action<SyncIOConnectedClient> afterSend, IPacket packet)
        {
            var data = Packager.Pack(packet, PackagingConfiguration);
            HandleRawBytes(data, afterSend);
        }

        public void SetIdentifier(Guid id)
        {
            Id = id;
        }

        #endregion

        #region Send

        /// <summary>
        /// Assigns a prefix to the data and adds it to the send queue.
        /// </summary>
        /// <param name="data">Data to send</param>
        private void HandleRawBytes(byte[] data, Action<SyncIOConnectedClient> afterSend)
        {
            // Appending length prefix to packet
            var packet = BitConverter.GetBytes(data.Length).Concat(data).ToArray();
            lock (_syncLock)
            {
                _sendQueue.Enqueue(new QueuedPacket(packet, afterSend));
                Task.Factory.StartNew(HandleSendQueue);
            }
        }

        private void HandleSendQueue()
        {
            QueuedPacket packet = null;

            lock (_syncLock)
            {
                packet = _sendQueue.Dequeue();
            }

            if (packet != null && packet.Data != null)
            {
                var socketError = SocketError.SocketError;
                NetworkSocket?.Send(packet.Data, 0, packet.Data.Length, SocketFlags.None, out socketError);
                if (socketError != SocketError.Success)
                {
                    Disconnect(new SocketException());
                    return;
                }
                else
                {
                    packet.HasBeenSent(this);
                }
            }
        }

        #endregion

        #region Receive

        public void BeginReceive(Action<InternalSyncIOConnectedClient, IPacket> callback)
        {
            if (_receiveCallback != null)
            {
                throw new Exception("Alredy listening.");
            }

            _receiveCallback = callback;
            if (_receiveCallback == null)
            {
                throw new Exception("Invalid callback");
            }

            ReceiveWithDefragger();
        }

        /// <summary>
        /// Uses defagger to receive packets.
        /// </summary>
        private void ReceiveWithDefragger()
        {
            NetworkSocket.BeginReceive(_defragger.ReceiveBuffer, _defragger.BufferIndex, _defragger.BytesToReceive, SocketFlags.None, out SocketError socketError, InternalReceive, null);

            if (socketError != SocketError.Success &&
                socketError != SocketError.IOPending)
            {
                Disconnect(new SocketException());
            }
        }

        private void InternalReceive(IAsyncResult at)
        {
            var socketError = SocketError.SocketError;
            var bytes = NetworkSocket?.EndReceive(at, out socketError) ?? 0;

            if (socketError != SocketError.Success)
            {
                // Client force closed connection
                Disconnect(new SocketException());
                return;
            }

            var packet = _defragger.Process(bytes);

            ReceiveWithDefragger();

            if (packet != null)
            {
                try
                {
                    var pack = Packager.Unpack(packet, PackagingConfiguration);
                    _receiveCallback(this, pack);
                }
                catch (Exception ex)
                {
                    Disconnect(ex);
                    return;
                }
            }
        }

        #endregion
    }
}