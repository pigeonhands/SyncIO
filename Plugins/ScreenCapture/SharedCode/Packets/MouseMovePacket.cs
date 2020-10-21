namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class MouseMovePacket : IPacket
    {
        public double X { get; set; }

        public double Y { get; set; }

        public int WheelData { get; set; }

        public MouseMovePacket(double x, double y, int wheelData)
        {
            X = x;
            Y = y;
            WheelData = wheelData;
        }
    }
}