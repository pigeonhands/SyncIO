namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class ChatMessage : IPacket
    {
        public string Message { get; set; }

        public ChatMessage(string msg)
        {
            Message = msg;
        }
    }
}