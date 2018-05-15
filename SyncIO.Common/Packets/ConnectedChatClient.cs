namespace SyncIO.Common.Packets
{
    using System;
    using System.Drawing;

    using SyncIO.Network;

    public class ConnectedChatClient
    {
        public SyncIOConnectedClient Connection { get; }

        public string OS { get; set; }

        public string UserName { get; set; }

        public string MachineName { get; set; }

        public Version ClientVersion { get; set; }

        public Image DesktopScreenshot { get; set; } 

        public bool CanUseTimeCommand { get; set; }

        public ConnectedChatClient(SyncIOConnectedClient conn, string os, string user, string machine, Version clientVersion)
        {
            Connection = conn;
            OS = os;
            UserName = user;
            MachineName = machine;
            ClientVersion = clientVersion;
        }
    }
}