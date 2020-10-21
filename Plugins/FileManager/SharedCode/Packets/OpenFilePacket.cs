namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class OpenFilePacket : IPacket
    {
        public string FilePath { get; set; }

        public OpenFilePacket(string path)
        {
            FilePath = path;
        }
    }
}