namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class RenameFilePacket : IPacket
    {
        public string FilePath { get; set; }

        public string OldFilePath { get; set; }

        public bool IsFolder { get; set; }

        public RenameFilePacket(string path, string oldPath, bool isFolder)
        {
            FilePath = path;
            OldFilePath = oldPath;
            IsFolder = isFolder;
        }
    }
}