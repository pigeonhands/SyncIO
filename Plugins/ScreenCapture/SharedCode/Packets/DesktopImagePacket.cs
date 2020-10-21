namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class DesktopImagePacket : IPacket
    {
        public byte[] Image { get; }

        public DesktopImagePacket(byte[] img)
        {
            Image = img;
        }
    }
}