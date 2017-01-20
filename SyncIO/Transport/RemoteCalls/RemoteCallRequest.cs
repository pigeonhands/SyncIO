using SyncIO.Transport.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.RemoteCalls {
    [Serializable]
    internal class RemoteCallRequest : IPacket {
        public string Name { get; set; }
        public object[] Args { get; set; }
        public Guid CallID { get; set; }
    }
}
