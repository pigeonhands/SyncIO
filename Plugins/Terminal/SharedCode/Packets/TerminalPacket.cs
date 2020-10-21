namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class TerminalPacket : IPacket
    {
        public string Input { get; set; }

        public TerminalPacket(string input)
        {
            Input = input;
        }
    }
}