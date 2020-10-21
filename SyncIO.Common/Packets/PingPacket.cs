namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class PingPacket : IPacket
    {
        public DateTime Timestamp { get; set; }

        public double Latency { get; set; }

        public PingPacket(DateTime timestamp)
        {
            Timestamp = timestamp;
        }

        public PingPacket(double latency)
        {
            Latency = latency;
        }
    }
}