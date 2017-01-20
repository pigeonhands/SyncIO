using SyncIO.Transport.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.RemoteCalls {

    [Serializable]
    internal class RemoteFunctionInfomation : IPacket {

        public RemoteFunctionResponce Reponce { get; set; }

        public string Name { get; set; }
        public uint[] Parameters { get; set; }
        public uint ReturnType { get; set; }

    }

    enum RemoteFunctionResponce : byte {
        DoesNotExist,
        Success,
        PermissionDenied
    }
}
