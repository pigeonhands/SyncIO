namespace SyncIO.Common.Packets
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;

    using SyncIO.Network;

    public class ConnectedChatClient
    {
        public SyncIOConnectedClient Connection { get; }

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

        public Image DesktopScreenshot { get; set; } 

        public bool CanUseTimeCommand { get; set; }

        public DateTime LastUpdated { get; set; }

        public double Latency { get; set; }

        public ConnectedChatClient(SyncIOConnectedClient conn, ClientInfoPacket packet)
        {
            Connection = conn;
            Update(packet);
        }

        public void Update(ClientInfoPacket packet)
        {
            OS = packet.OS;
            GroupName = packet.GroupName;
            OsPlatform = packet.OsPlatform;
            OsArchitecture = packet.OsArchitecture;
            TotalMemory = packet.TotalMemory;
            UserName = packet.UserName;
            MachineName = packet.MachineName;
            ProcessorName = packet.ProcessorName;
            ClientVersion = packet.ClientVersion;
            Uptime = packet.Uptime;
            LastUpdated = packet.LastUpdated;
            if (packet.DesktopData != null)
            {
                DesktopScreenshot = ByteArrayToImage(packet.DesktopData);
            }
        }

        public static Image ByteArrayToImage(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }
    }
}