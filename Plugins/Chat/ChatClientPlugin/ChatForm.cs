namespace ChatClientPlugin
{
    using System;
    using System.Windows.Forms;

    using SyncIO.ClientPlugin;
    using SyncIO.Common.Packets;
    using SyncIO.Network;

    public partial class ChatForm : Form
    {
        private readonly ISyncIOClient _client;

        private readonly INetHost _netHost;

        public ChatForm(INetHost netHost, ISyncIOClient client)
        {
            InitializeComponent();

            _netHost = netHost;
            _client = client;

            _netHost.SetHandler<ChatPacket>((c, p) => HandleChatPacket(p));
        }

        private void HandleChatPacket(ChatPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => HandleChatPacket(packet)));
                return;
            }

            richTextBox1.AppendText(packet.Message);
            richTextBox1.AppendText(Environment.NewLine);
        }
    }
}