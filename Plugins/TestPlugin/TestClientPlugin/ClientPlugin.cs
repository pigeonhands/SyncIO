namespace TestClientPlugin
{
    using System;

    using SyncIO.Client;
    using SyncIO.ClientPlugin;
    using SyncIO.Common.Packets;
    using SyncIO.Transport.Packets;

    public class ClientPlugin : ISyncIOClientPlugin
    {
        #region Variables

        private SyncIOClient _clientHost;
        private readonly INetHost _netHost;
        private readonly ILoggingHost _loggingHost;

        #endregion

        #region Constructor

        public ClientPlugin(INetHost netHost, ILoggingHost loggingHost)
        {
            _netHost = netHost;
            _loggingHost = loggingHost;

            _netHost.SetHandler<TaskManagerInfo>(TaskManagerInfo);
        }

        #endregion

        #region Events

        public void OnPluginReady(SyncIOClient client)
        {
            _loggingHost.Trace($"OnPluginReady [ClientHost={client.Id}]");

            _clientHost = client;
        }

        public void OnConnect()
        {
            _loggingHost.Trace("OnConnect");
        }

        public void OnDisconnect(Exception error)
        {
            _loggingHost.Trace($"OnDisconnect [Error={error}]");
        }

        public void OnPacketReceived(IPacket packet)
        {
            _loggingHost.Trace($"OnPacketReceived [Packet={packet}]");
        }

        #endregion

        private void TaskManagerInfo(SyncIOClient client, TaskManagerInfo packet)
        {
            Console.WriteLine($"TaskManagerInfo {client.Id}");
        }
    }
}