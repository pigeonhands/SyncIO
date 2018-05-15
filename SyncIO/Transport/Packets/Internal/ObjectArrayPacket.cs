namespace SyncIO.Transport.Packets.Internal
{
    using System;

    [Serializable]
    internal class ObjectArrayPacket : IPacket
    {
        public object[] Data { get; set; }

        public ObjectArrayPacket(object[] data)
        {
            Data = data;
        }
    }
}