namespace ChatClientPlugin
{
    using System;
    using System.Windows.Forms;

    using SyncIO.Client;
    using SyncIO.ClientPlugin;
    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.Transport.Packets;

    public class ClientPlugin : ApplicationContext, ISyncIOClientPlugin
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

            
            Start();
        }

        [STAThread]
        private void Start()
        {
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Chat packet handlers
            var chatForm = new ChatForm(_netHost, _clientHost);
            _netHost.SetHandler<StartChatPacket>((c, p) =>
            {
                if (chatForm.InvokeRequired)
                    chatForm.Invoke(new MethodInvoker(() => chatForm.Show()));
                else
                    chatForm.Show();
            });
            _netHost.SetHandler<StopChatPacket>((c, p) =>
            {
                if (chatForm.InvokeRequired)
                    chatForm.Invoke(new MethodInvoker(() => chatForm.Dispose()));
                else
                    chatForm.Dispose();
            });
            Application.Run();
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