namespace SyncIO.Common.Packets
{
    using System;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class ClientInfo : IPacket
    {
        public string OS { get; set; }

        public string UserName { get; set; }

        public string MachineName { get; set; }

        public Version ClientVersion { get; set; }

        public ClientInfo(string os, string userName, string machineName, Version clientVersion)
        {
            OS = os;
            UserName = userName;
            MachineName = machineName;
            ClientVersion = clientVersion;
        }
    }
}