namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class AuthPacket : IPacket
    {
        public byte[] Key { get; set; }

        public byte[] IV { get; set; }

        public AuthPacket(byte[] key, byte[] iv)
        {
            Key = key;
            IV = iv;
        }
    }
}