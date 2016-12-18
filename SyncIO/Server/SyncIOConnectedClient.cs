using SyncIO.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Server {
    /// <summary>
    /// A client that is connected to a SyncIOServer
    /// Used from receving/sending from the SyncIOServer.
    /// </summary>
    public abstract class SyncIOConnectedClient {
        public Guid ID { get; protected set; }
        public virtual void Send(params object[] data) {
        }
    }

    /// <summary>
    /// Used to create SyncIOConnectedClient objects. 
    /// </summary>
    internal class InternalSyncIOConnectedClient : SyncIOConnectedClient {

        /// <summary>
        /// Underlying socket connection for the client
        /// </summary>
        public Socket NetworkSocket { get; }
        public Packager Packager { get; }
        public PackConfig PackagingConfiguration { get; set; }

        /// <summary>
        /// Multithread sync object
        /// </summary>
        private object SyncLock = new object();

        /// <summary>
        /// Send queue for client
        /// </summary>
        private Queue<byte[]> SendQueue = new Queue<byte[]>();

        /// <summary>
        /// Used to handle receving of data.
        /// </summary>
        private PacketDefragmenter Defragger;

        public InternalSyncIOConnectedClient(Socket s, Packager p, int bufferSize) {
            NetworkSocket = s;
            Packager = p;
            Defragger = new PacketDefragmenter(bufferSize);
        }

        public InternalSyncIOConnectedClient(Socket s, Packager p) : this(s, p, 1024 * 5) {
        }

        public override void Send(params object[] arr) {
            byte[] data = Packager.PackArray(arr, PackagingConfiguration);
            byte[] packet = BitConverter.GetBytes(data.Length).Concat(data).ToArray();//Appending length prefix to packet
            lock (SyncLock) {
                SendQueue.Enqueue(packet);
                Task.Factory.StartNew(HandleSendQueue);
            }
        }


        private void HandleSendQueue() {
            byte[] packet = null;

            lock (SyncLock) {
                packet = SendQueue.Dequeue();
            }

            if(packet != null)
                NetworkSocket.Send(packet);
        }

    }
}
