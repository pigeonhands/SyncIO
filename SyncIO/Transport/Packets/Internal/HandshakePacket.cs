namespace SyncIO.Transport.Packets.Internal
{
    using System;

    [Serializable]
    internal class HandshakePacket : IPacket
    {
        public Guid Id { get; set; }

        public bool Success { get; set; }

        public HandshakePacket(Guid id, bool success)
        {
            Id = id;
            Success = success;
        }
    }
}