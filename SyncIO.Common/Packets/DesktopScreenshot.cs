namespace SyncIO.Common.Packets
{
    using System;
    using System.Drawing;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class DesktopScreenshot : IPacket
    {
        public byte[] Image { get; }

        public DesktopScreenshot(byte[] img)
        {
            Image = img;
        }
    }
}