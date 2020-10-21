namespace FileManagerServerPlugin
{
    using System;
    using System.Windows.Forms;

    using SyncIO.Network;
    using SyncIO.ServerPlugin;
    using SyncIO.Transport.Packets;

    using FileManagerServerPlugin.UI.Forms;

    public class ServerPlugin : ISyncIOServerPlugin, IUICallbacks
    {
        private readonly IUIHost _uiHost;
        private readonly INetHost _netHost;
        private readonly ILoggingHost _loggingHost;

        public string Name => "File Manager";

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
                Name = "File Manager",
                OnClick = (sender, e) =>
                {
                    foreach (var client in e)
                    {
                        var fileManager = new FileManager(_netHost, client.Value);
                        fileManager.Tag = client.Value.Id;
                        fileManager.Show();
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
            var openForms = Application.OpenForms;
            for (var i = 0; i < openForms.Count; i++)
            {
                var form = openForms[i];
                if (!(form.Tag is Guid id))
                    continue;

                if (id != client.Id)
                    continue;

                if (form.InvokeRequired)
                    form.Invoke(new MethodInvoker(() => form.Dispose()));
                else
                    form.Dispose();
            }
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