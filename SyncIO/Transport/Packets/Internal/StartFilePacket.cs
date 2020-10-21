namespace SyncIO.Transport.Packets.Internal
{
    using System;

    [Serializable]
    public class StartFilePacket : IPacket
    {
        public string FilePath { get; set; }

        public FileTransferType TransferType { get; set; }

        public StartFilePacket(string path, FileTransferType type)
        {
            FilePath = path;
            TransferType = type;
        }
    }
}