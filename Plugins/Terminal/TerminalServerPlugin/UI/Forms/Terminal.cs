namespace TerminalServerPlugin.UI.Forms
{
    using System;
    using System.Windows.Forms;

    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.ServerPlugin;

    public partial class Terminal : Form
    {
        private readonly INetHost _netHost;
        private readonly ISyncIOClient _client;

        public Terminal(INetHost netHost, ISyncIOClient client)
        {
            InitializeComponent();

            _netHost = netHost;
            _client = client;

            _netHost.SetHandler<TerminalPacket>((c, p) => HandleCommandPacket(p));

            _client.Send(new StartTerminalPacket());
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            _client.Send(new StopTerminalPacket());
            e.Cancel = false;
        }

        private void CommandPrompt1_Command(object sender, CommandEventArgs e)
        {
            switch (e.Command)
            {
                case "exit":
                case "quit":
                case "terminate":
                case "bye":
                case "byebye":
                case "byte":
                case "byet":
                    _client.Send(new StopTerminalPacket());
                    Dispose();
                    break;
                default:
                    _client.Send(new TerminalPacket(e.Command));
                    break;
            }
        }

        private void HandleCommandPacket(TerminalPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => HandleCommandPacket(packet)));
                return;
            }

            if (string.IsNullOrEmpty(packet.Input))
                return;

            if (commandPrompt1 != null)
            {
                commandPrompt1.AddMessage(packet.Input);
            }
        }
    }
}