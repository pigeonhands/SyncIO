using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Network.Callbacks {
    internal class QueuedPacket {
        public byte[] Data { get; }
        private Action<InternalSyncIOConnectedClient> AfterSend;
        public QueuedPacket(byte[] _data, Action<InternalSyncIOConnectedClient> _after) {
            Data = _data;
            AfterSend = _after;
        }

        public QueuedPacket(byte[] _data) : this(_data, null) {
        }

        public void HasBeenSent(InternalSyncIOConnectedClient sender) {
            AfterSend?.Invoke(sender);
        }
    }
}
