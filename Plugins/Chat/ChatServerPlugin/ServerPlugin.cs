namespace ChatServerPlugin
{
    using System;

    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.ServerPlugin;
    using SyncIO.Transport.Packets;

    //using ChatServerPlugin.UI.Forms;

    public class ServerPlugin : ISyncIOServerPlugin, IUICallbacks
    {
        private readonly IUIHost _uiHost;
        private readonly INetHost _netHost;
        private readonly ILoggingHost _loggingHost;

        public string Name => "Chat";

        public string Author => "versx";

        public ServerPlugin(IUIHost uiHost, INetHost netHost, ILoggingHost loggingHost)
        {
            _uiHost = uiHost;
            _netHost = netHost;
            _loggingHost = loggingHost;
        }

        public void OnPluginReady()
        {
            _loggingHost.Trace($"OnPluginReady");
            var contextEntry = new ContextEntry
            {
                Name = "Chat",
                OnClick = (sender, e) =>
                {
                    foreach (var client in e)
                    {
                        // TODO: Open server chat form
                        client.Value.Send(new StartChatPacket());
                    }
                }
            };
            _uiHost.AddContextMenuEntry(contextEntry);
            _loggingHost.Debug($"Added context entry {contextEntry.Name}...");
        }

        public void OnClientConnect(ISyncIOClient client)
        {
            _loggingHost.Trace($"OnClientConnect [Client={client.EndPoint}]");
        }

        public void OnClientDisconnect(ISyncIOClient client)
        {
            _loggingHost.Trace($"OnClientDisconnect [Client={client.EndPoint}]");
            // TODO: Close open forms
        }

        public void OnPacketReceived(ISyncIOClient client, IPacket packet)
        {
            _loggingHost.Trace($"OnPacketReceived [Client={client.EndPoint}, Packet={packet}]");
        }

        public void OnInvalidated()
        {
            _loggingHost.Trace($"OnInvalidated");
        }
    }
}
