using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Network.Callbacks {
    internal class QueuedPacket {
        public byte[] Data { get; }
        private Action<SyncIOConnectedClient> AfterSend;
        public QueuedPacket(byte[] _data, Action<SyncIOConnectedClient> _after) {
            Data = _data;
            AfterSend = _after;
        }

        public QueuedPacket(byte[] _data) : this(_data, null) {
        }

        public void HasBeenSent(SyncIOConnectedClient sender) {
            AfterSend?.Invoke(sender);
        }
    }
}
