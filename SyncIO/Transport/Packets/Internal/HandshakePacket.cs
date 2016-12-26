using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.Packets.Internal {
    [Serializable]
    internal class HandshakePacket : IPacket {
        public bool Success { get; set; }
        public Guid ID { get; set; }

        public HandshakePacket(bool _success, Guid _id) {
            Success = _success;
            ID = _id;
        }
    }
}
