namespace SyncIO.Transport.RemoteCalls
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    internal class RemoteFunctionInfomation : IPacket
    {
        public RemoteFunctionResponse Response { get; set; }

        public string Name { get; set; }

        public uint[] Parameters { get; set; }

        public uint ReturnType { get; set; }
    }

    enum RemoteFunctionResponse : byte
    {
        DoesNotExist,
        Success,
        PermissionDenied
    }
}