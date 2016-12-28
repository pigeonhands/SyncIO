using SyncIO.Network;
using SyncIO.Network.Callbacks;
using SyncIO.Transport;
using SyncIO.Transport.Encryption;
using SyncIO.Transport.Packets;
using SyncIO.Transport.Packets.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Network {
    public delegate void OnClientDisconnectDelegate(SyncIOConnectedClient client, Exception ex);

    /// <summary>
    /// A client that is connected to a SyncIOServer
    /// Used from receving/sending from the SyncIOServer.
    /// </summary>
    public abstract class SyncIOConnectedClient : ISyncIOClient {
        public event OnClientDisconnectDelegate OnDisconnect;
        /// <summary>
        /// Id of connected client.
        /// </summary>
        public Guid ID { get; protected set; }

        /// <summary>
        /// Underlying socket connection for the client
        /// </summary>
        protected Socket NetworkSocket { get; set; }

        internal PackConfig PackagingConfiguration { get; set; }


        public void Send(params object[] data) {
            Send(null, data);
        }

        public void Send(IPacket packet) {
            Send(null, packet);
        } 

        public virtual void Send(Action<SyncIOConnectedClient> afterSend, params object[] data) {
        }

        public virtual void Send(Action<SyncIOConnectedClient> afterSend, IPacket packet) {
        }

        protected void Disconnect(Exception ex) {
            if(NetworkSocket != null) {
                NetworkSocket.Shutdown(SocketShutdown.Both);
                NetworkSocket.Dispose();
                NetworkSocket = null;
            }
            OnDisconnect?.Invoke(this, ex);
        }

        /// <summary>
        /// Sets the encryption for traffic.
        /// </summary>
        /// <param name="encryption">Encryption to use.</param>
        public void SetEncryption(ISyncIOEncryption encryption) {
            if (PackagingConfiguration == null)
                PackagingConfiguration = new PackConfig();

            PackagingConfiguration.Encryption = encryption;
        }
    }

    /// <summary>
    /// Used to create SyncIOConnectedClient objects. 
    /// </summary>
    internal class InternalSyncIOConnectedClient : SyncIOConnectedClient {

       
        public Packager Packager { get; }

        /// <summary>
        /// Multithread sync object
        /// </summary>
        private object SyncLock = new object();

        /// <summary>
        /// Send queue for client
        /// </summary>
        private Queue<QueuedPacket> SendQueue = new Queue<QueuedPacket>();

        /// <summary>
        /// Used to handle receving of data.
        /// </summary>
        private PacketDefragmenter Defragger;


        private Action<InternalSyncIOConnectedClient, IPacket> ReceveCallback;

        public InternalSyncIOConnectedClient(Socket s, Packager p, int bufferSize) {
           if (s == null)
                throw new Exception("Socket not valid.");

            NetworkSocket = s;
            Packager = p;
            Defragger = new PacketDefragmenter(bufferSize);
        }

        public InternalSyncIOConnectedClient(Socket s, Packager p) : this(s, p, 1024 * 5) {
        }


        public override void Send(Action<SyncIOConnectedClient> afterSend, object[] arr) {
            byte[] data = Packager.PackArray(arr, PackagingConfiguration);
            HandleRawBytes(data, afterSend);
        }

        public override void Send(Action<SyncIOConnectedClient> afterSend, IPacket packet) {
            byte[] data = Packager.Pack(packet);
            HandleRawBytes(data, afterSend);
        }

        /// <summary>
        /// Assigns a prefix to the data and adds ti to the send queue.
        /// </summary>
        /// <param name="data">Data to send</param>
        private void HandleRawBytes(byte[] data, Action<SyncIOConnectedClient> afterSend) {
            byte[] packet = BitConverter.GetBytes(data.Length).Concat(data).ToArray();//Appending length prefix to packet
            lock (SyncLock) {
                SendQueue.Enqueue(new QueuedPacket(packet, afterSend));
                Task.Factory.StartNew(HandleSendQueue);
            }
        }

        private void HandleSendQueue() {
            QueuedPacket packet = null;

            lock (SyncLock) {
                packet = SendQueue.Dequeue();
            }

            if(packet != null && packet.Data != null) {
                SocketError SE;

                NetworkSocket.Send(packet.Data, 0, packet.Data.Length, SocketFlags.None, out SE);

                if (SE != SocketError.Success) {
                    Disconnect(new SocketException());
                    return;
                }else {
                    packet.HasBeenSent(this);
                }
            }
        }

       

        public void BeginReceve(Action<InternalSyncIOConnectedClient, IPacket> callback) {

            if (ReceveCallback != null)
                throw new Exception("Alredy listening.");

            ReceveCallback = callback;
            if (ReceveCallback == null)
                throw new Exception("Invalid callback");

            ReceveWithDefragger();
        }

        /// <summary>
        /// Uses defagger to receve packets.
        /// </summary>
         private void ReceveWithDefragger() {
            SocketError SE;
            NetworkSocket.BeginReceive(Defragger.ReceveBuffer, Defragger.BufferIndex, Defragger.BytesToReceve, SocketFlags.None, out SE, InternalReceve, null);

            if (SE != SocketError.Success)
                Disconnect(new SocketException());
        }

        private void InternalReceve(IAsyncResult AR) {
            SocketError SE;
            int bytes = NetworkSocket.EndReceive(AR, out SE);

            if (SE != SocketError.Success) {
                Disconnect(new SocketException());
                return;
            }

            byte[] packet = Defragger.Process(bytes);
            if (packet != null) {
                try {
                    IPacket pack = Packager.Unpack(packet, PackagingConfiguration);
                    ReceveCallback(this, pack);
                } catch (Exception ex){
                    Disconnect(ex);
                    return;
                }
            }

            ReceveWithDefragger();
        }

        public void SetID(Guid _id) {
            ID = _id;
        }

    }
}
