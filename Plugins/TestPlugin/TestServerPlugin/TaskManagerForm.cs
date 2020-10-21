namespace TestServerPlugin
{
    using System;
    using System.Windows.Forms;

    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.ServerPlugin;

    public partial class TaskManagerForm : Form
    {
        private readonly INetHost _netHost;
        private readonly ISyncIOClient _client;

        public TaskManagerForm(INetHost netHost, ISyncIOClient client)
        {
            InitializeComponent();

            _netHost = netHost;
            _client = client;

            _netHost.SetHandler<TaskManagerInfo>(TaskManagerInfo);
        }

        private void TaskManagerInfo(SyncIOConnectedClient client, TaskManagerInfo packet)
        {
            MessageBox.Show($"TaskManagerInfo: {client.Id}");
            Console.WriteLine($"TaskManagerInfo: {client.Id}");
        }
    }
}