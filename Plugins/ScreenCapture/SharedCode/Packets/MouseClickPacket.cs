namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class MouseClickPacket : IPacket
    {
        public double X { get; set; }

        public double Y { get; set; }

        public int WheelDelta { get; set; }

        public bool IsKeyUp { get; set; }

        public bool IsLeftClick { get; set; }

        public MouseClickPacket(double x, double y, int wheelDelta, bool isKeyUp, bool isLeftClick)
        {
            X = x;
            Y = y;
            WheelDelta = wheelDelta;
            IsKeyUp = isKeyUp;
            IsLeftClick = isLeftClick;
        }
    }
}