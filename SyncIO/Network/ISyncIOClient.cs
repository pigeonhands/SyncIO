using SyncIO.Transport.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Network {
    public interface ISyncIOClient {
        void Send(params object[] data);
        void Send(IPacket packet);
    }
}
