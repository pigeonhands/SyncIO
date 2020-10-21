namespace SyncIO.Common.Packets
{
    using System;
    using System.Runtime.InteropServices;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class ClientInfoPacket : IPacket
    {
        public byte[] DesktopData { get; set; }

        public string GroupName { get; set; }

        public string OS { get; set; }

        public OsPlatform OsPlatform { get; set; }

        public Architecture OsArchitecture { get; set; }

        public string UserName { get; set; }

        public string MachineName { get; set; }

        public string ProcessorName { get; set; }

        public long TotalMemory { get; set; }

        public Version ClientVersion { get; set; }

        public TimeSpan Uptime { get; set; }

        public DateTime LastUpdated { get; set; }

        public ClientInfoPacket()
        {
            LastUpdated = DateTime.Now;
        }
    }
}