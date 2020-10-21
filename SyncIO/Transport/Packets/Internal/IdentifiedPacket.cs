namespace SyncIO.Transport.Packets.Internal
{
    using System;

    /// <summary>
    /// Primarily for UDP
    /// </summary>
    [Serializable]
    internal class IdentifiedPacket : IPacket
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