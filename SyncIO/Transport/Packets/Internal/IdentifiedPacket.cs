namespace SyncIO.Transport.Packets.Internal
{
    using System;

    [Serializable]
    internal class IdentifiedPacket : IPacket //Primaraly for UDP
    {
        public Guid Id { get; set; }

        public IPacket Packet { get; set; }

        public IdentifiedPacket(Guid id, IPacket packet)
        {
            Id = id;
            Packet = packet;
        }
    }
}