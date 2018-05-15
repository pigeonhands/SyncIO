namespace SyncIO.Transport.RemoteCalls
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    internal class RemoteCallRequest : IPacket
    {
        public string Name { get; set; }

        public object[] Args { get; set; }

        public Guid CallId { get; set; }
    }
}