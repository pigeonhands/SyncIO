using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.Packets.Internal {
    internal class ObjectArrayPacket {
        public object[] Data { get; set; }
        public ObjectArrayPacket(object[] d) {
            Data = d;
        }
    }
}
