namespace TestServerPlugin
{
    using System;
    using System.Collections.Generic;

    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.ServerPlugin;
    using SyncIO.Transport.Packets;

    //using SharedCode.Headers;

    public class ServerPlugin : ISyncIOServerPlugin, IUICallbacks
    {
        private readonly IUIHost _uiHost;
        private readonly INetHost _netHost;
        private readonly ILoggingHost _loggingHost;

        private ColumnEntry _testColumn;

        public string Name => "Test Plugin";

        public string Author => "versx";

        public ServerPlugin(IUIHost uiHost, INetHost netHost, ILoggingHost loggingHost)
        {
            _uiHost = uiHost;
            _netHost = netHost;
            _loggingHost = loggingHost;
        }
        
        public void OnPluginReady()
        {
            _netHost.SetHandler<TaskManagerInfo>(TaskManagerInfo);

            _loggingHost.Trace($"OnPluginReady");

            var contextEntry = new ContextEntry
            {
                Name = "Test",
                Children = new List<ContextEntry>
                {
                    new ContextEntry
                    {
                        Name = "Test1",
                        OnClick = (sender, e) => 
                        {
                            //Console.WriteLine($"Clicked! {e.Count}");
                            foreach (var client in e)
                            {
                                var taskMgrForm = new TaskManagerForm(_netHost, client.Value);
                                taskMgrForm.Show();
                                client.Value.Send(new TaskManagerInfo());
                            }
                        }
                    }
                }
            };
            _uiHost.AddContextMenuEntry(contextEntry);
            _loggingHost.Debug($"Added context entry {contextEntry.Name}...");

            _uiHost.AddColumnEntry(_testColumn = new ColumnEntry { Text = "Test Column", Width = 100 });
            _loggingHost.Debug($"Added column entry {_testColumn.Text}...");
        }

        public void OnClientConnect(ISyncIOClient client)
        {
            _loggingHost.Trace($"OnClientConnect [Client={client.EndPoint}]");

            _loggingHost.Info($"Client {client.Id} connected from end point {client.EndPoint}...");

            var text = "Test Data";
            _uiHost.SetColumnValue(client.Id, _testColumn, text);
            _loggingHost.Debug($"Setting column value for client {client.Id} to {text}");
        }

        public void OnClientDisconnect(ISyncIOClient client)
        {
            _loggingHost.Trace($"OnClientDisconnect [Client={client.EndPoint}]");
        }

        public void OnPacketReceived(ISyncIOClient client, IPacket packet)
        {
            _loggingHost.Trace($"OnPacketReceived [Client={client.EndPoint}, Packet={packet}]");
        }

        public void OnInvalidated()
        {
            _loggingHost.Trace($"OnInvalidated");
        }

        private void TaskManagerInfo(SyncIOConnectedClient client, TaskManagerInfo packet)
        {
            Console.WriteLine($"TaskManagerInfo {client.Id}");
        }
    }
}