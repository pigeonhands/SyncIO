namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class ChatPacket : IPacket
    {
        public string Message { get; set; }

        public ChatPacket(string message)
        {
            Message = message;
        }
    }
}