using SyncIO.Transport.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.RemoteCalls {
    [Serializable]
    internal class RemoteCallResponse : IPacket {
        public string Name { get; set; } //replace names with IDs
        public FunctionResponceStatus Reponce { get; set; }
        public object Return { get; set; }
        public Guid CallID { get; set; }

        public RemoteCallResponse(Guid _callId, string _name) {
            CallID = _callId;
            Name = _name;
        }
    }


    public enum FunctionResponceStatus : byte {
        Success,
        PermissionDenied,
        ExceptionThrown,
        DoesNotExist,
        InvalidParameters
    }
}
