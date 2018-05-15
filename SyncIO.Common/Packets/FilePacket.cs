namespace SyncIO.Common.Packets
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class FilePacket : IPacket
    {
        public string FileName { get; set; }

        public List<byte[]> Bytes { get; set; }

        public FilePacket(string filePath)
        {
            FileName = Path.GetFileName(filePath);
            Bytes = new List<byte[]>();
            for (int i = 0; i < 5; i++)
            {
                Bytes.Add(File.ReadAllBytes(filePath));
            }
        }
    }
}