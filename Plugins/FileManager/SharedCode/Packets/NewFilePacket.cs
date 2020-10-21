namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class NewFilePacket : IPacket
    {
        public string FilePath { get; set; }

        public bool IsFolder { get; set; }

        public NewFilePacket(string path, bool isFolder)
        {
            FilePath = path;
            IsFolder = isFolder;
        }
    }
}