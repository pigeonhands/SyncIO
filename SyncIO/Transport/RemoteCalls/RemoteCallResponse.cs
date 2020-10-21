namespace SyncIO.Transport.RemoteCalls
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    internal class RemoteCallResponse : IPacket
    {
        public string Name { get; set; } // Replace names with IDs

        public FunctionResponseStatus Response { get; set; }

        public object Return { get; set; }

        public Guid CallId { get; set; }

        public RemoteCallResponse(Guid callId, string name)
        {
            CallId = callId;
            Name = name;
        }
    }

    public enum FunctionResponseStatus : byte
    {
        Success,
        PermissionDenied,
        ExceptionThrown,
        DoesNotExist,
        InvalidParameters
    }
}