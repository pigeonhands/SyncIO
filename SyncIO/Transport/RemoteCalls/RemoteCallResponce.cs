using SyncIO.Transport.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.RemoteCalls {
    [Serializable]
    internal class RemoteCallResponce : IPacket {
        public FunctionResponceStatus Reponce { get; set; }
        public object Return { get; set; }
    }


    internal enum FunctionResponceStatus : byte {
        Success,
        PermissionDenied,
        ExceptionThrown,
        DoesNotExist,
        InvalidParameters
    }
}
