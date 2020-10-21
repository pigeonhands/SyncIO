namespace SyncIO.Transport.Packets.Internal
{
    using System;

    [Serializable]
    public class FileDataPacket : IPacket
    {
        // Handle, maybe use file path instead
        public Guid Id { get; set; }

        public string FilePath { get; set; }

        public byte[] Block { get; set; }

        public long Offset { get; set; }

        public bool FinalBlock { get; set; }

        public FileDataPacket(Guid id, string path, byte[] block, long offset, bool finalBlock)
        {
            Id = id;
            FilePath = path;
            Block = block;
            Offset = offset;
            FinalBlock = finalBlock;
        }
    }
}
/*
 * Send data block
 * Write data from index/offset to data block length
 * If final block, close stream and remove transfer - set complete
 */