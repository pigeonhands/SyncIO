namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class ClientOptionPacket : IPacket
    {
        public ClientOptions Option { get; set; }

        public ClientOptionPacket(ClientOptions option)
        {
            Option = option;
        }
    }

    [Serializable]
    public enum ClientOptions
    {
        Disconnect,
        Reconnect,
        RestartApp,
        CloseApp
    }
}