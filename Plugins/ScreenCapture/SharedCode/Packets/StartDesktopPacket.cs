namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class StartDesktopPacket : IPacket
    {
        public int Quality { get; set; }

        public int MaxFPS { get; set; }

        public StartDesktopPacket(int quality, int maxFPS)
        {
            Quality = quality;
            MaxFPS = maxFPS;
        }
    }
}