using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.Packets.Internal {
    [Serializable]
    internal class UdpHandshakeResponce : IPacket {

        public bool Success { get; set; }

    }
}
