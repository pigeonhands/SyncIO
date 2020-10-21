namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class DeleteFilePacket : IPacket
    {
        public string FilePath { get; set; }

        public bool IsFolder { get; set; }

        public DeleteFilePacket(string path, bool isFolder)
        {
            FilePath = path;
            IsFolder = isFolder;
        }
    }
}