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
        public Guid ID { get; protected set; }

        /// <summary>
        /// General tag data
        /// not used internaly by SyncIO.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Remote EndPoint of client
        /// </summary>
        public IPEndPoint EndPoint => (IPEndPoint)NetworkSocket.RemoteEndPoint;

        /// <summary>
        /// Underlying socket connection for the client
        /// </summary>
        protected Socket NetworkSocket { get; set; }

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
        /// Sets the encryption for traffic.
        /// </summary>
        /// <param name="encryption">Encryption to use.</param>
        public void SetEncryption(ISyncIOEncryption encryption)
        {
            if (PackagingConfiguration == null)
                PackagingConfiguration = new PackConfig();

            PackagingConfiguration.Encryption = encryption;
        }

        public void SetCompression(ISyncIOCompression compression)
        {
            if (PackagingConfiguration == null)
                PackagingConfiguration = new PackConfig();

            PackagingConfiguration.Compression = compression;
        }
    }

    /// <summary>
    /// Used to create SyncIOConnectedClient objects. 
    /// </summary>
    internal class InternalSyncIOConnectedClient : SyncIOConnectedClient
    {
        /// <summary>
        /// Multithread sync object
        /// </summary>
        private readonly object SyncLock = new object();

        /// <summary>
        /// Send queue for client
        /// </summary>
        private Queue<QueuedPacket> SendQueue = new Queue<QueuedPacket>();

        /// <summary>
        /// Used to handle receving of data.
        /// </summary>
        private PacketDefragmenter Defragger;


        private Action<InternalSyncIOConnectedClient, IPacket> ReceveCallback;

        public Packager Packager { get; }


        public InternalSyncIOConnectedClient(Socket s, Packager p) 
            : this(s, p, 1024 * 5)
        {
        }

        public InternalSyncIOConnectedClient(Socket s, Packager p, int bufferSize)
        {
            NetworkSocket = s ?? throw new Exception("Socket not valid.");
            Packager = p;
            Defragger = new PacketDefragmenter(bufferSize);
        }

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

        /// <summary>
        /// Assigns a prefix to the data and adds ti to the send queue.
        /// </summary>
        /// <param name="data">Data to send</param>
        private void HandleRawBytes(byte[] data, Action<SyncIOConnectedClient> afterSend)
        {
            var packet = BitConverter.GetBytes(data.Length).Concat(data).ToArray();//Appending length prefix to packet
            lock (SyncLock)
            {
                SendQueue.Enqueue(new QueuedPacket(packet, afterSend));
                Task.Factory.StartNew(HandleSendQueue);
            }
        }

        private void HandleSendQueue()
        {
            QueuedPacket packet = null;

            lock (SyncLock)
            {
                packet = SendQueue.Dequeue();
            }

            if (packet != null && packet.Data != null)
            {
                var se = SocketError.SocketError;

                NetworkSocket?.Send(packet.Data, 0, packet.Data.Length, SocketFlags.None, out se);

                if (se != SocketError.Success)
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

        public void BeginReceve(Action<InternalSyncIOConnectedClient, IPacket> callback)
        {

            if (ReceveCallback != null)
                throw new Exception("Alredy listening.");

            ReceveCallback = callback;
            if (ReceveCallback == null)
                throw new Exception("Invalid callback");

            ReceiveWithDefragger();
        }

        /// <summary>
        /// Uses defagger to receve packets.
        /// </summary>
        private void ReceiveWithDefragger()
        {
            NetworkSocket.BeginReceive(Defragger.ReceveBuffer, Defragger.BufferIndex, Defragger.BytesToReceve, SocketFlags.None, out SocketError SE, InternalReceve, null);

            if (SE != SocketError.Success)
                Disconnect(new SocketException());
        }

        private void InternalReceve(IAsyncResult at)
        {
            var se = SocketError.SocketError;
            var bytes = NetworkSocket?.EndReceive(at, out se) ?? 0;

            if (se != SocketError.Success)
            {
                Disconnect(new SocketException());
                return;
            }

            var packet = Defragger.Process(bytes);

            ReceiveWithDefragger();

            if (packet != null)
            {
                try
                {
                    var pack = Packager.Unpack(packet, PackagingConfiguration);
                    ReceveCallback(this, pack);
                }
                catch (Exception ex)
                {
                    Disconnect(ex);
                    return;
                }
            }
        }

        public void SetID(Guid id)
        {
            ID = id;
        }
    }
}