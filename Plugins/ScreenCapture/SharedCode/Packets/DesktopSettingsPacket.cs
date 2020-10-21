namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class DesktopSettingsPacket : IPacket
    {
        public int ImageQuality { get; set; }

        public int MaxFPS { get; set; }

        public DesktopSettingsPacket(int quality, int maxFPS)
        {
            ImageQuality = quality;
            MaxFPS = maxFPS;
        }
    }
}