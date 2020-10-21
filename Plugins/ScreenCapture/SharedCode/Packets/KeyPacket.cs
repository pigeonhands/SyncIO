namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class KeyPacket : IPacket
    {
        public string Key { get; set; }

        public int Modifiers { get; set; }

        public bool IsKeyDown { get; set; }

        public KeyPacket(string key, int modifiers, bool isKeyDown)
        {
            Key = key;
            Modifiers = modifiers;
            IsKeyDown = isKeyDown;
        }
    }
}