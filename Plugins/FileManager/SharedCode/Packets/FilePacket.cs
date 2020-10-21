namespace SyncIO.Common.Packets
{
    using System;
    using System.Collections.Generic;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class FilePacket : IPacket
    {
        public string FilePath { get; set; }

        public List<FileInfo> Files { get; set; }

        public FilePacket(string path)
        {
            FilePath = path;
            Files = new List<FileInfo>();
        }
    }

    [Serializable]
    public class FileInfo
    {
        public string FilePath { get; set; }

        public long Size { get; set; }

        public string Type { get; set; }

        public bool IsHiddenFile { get; set; }

        public bool IsSystemFile { get; set; }

        public bool IsFolder => string.Compare(Type, "dir", true) == 0;
    }
}