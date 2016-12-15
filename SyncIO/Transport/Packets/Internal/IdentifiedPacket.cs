using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.Packets.Internal {
    internal class IdentifiedPacket { //Primaraly for UDP
        public Guid ID { get; set; }
        public IPacket Packet { get; set; }
        public IdentifiedPacket(Guid _id, IPacket _packet) {
            ID = _id;
            Packet = _packet;
        }
    }
}
