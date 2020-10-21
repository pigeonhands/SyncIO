namespace TaskManagerServerPlugin.UI.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.ServerPlugin;

    public partial class TaskManager : Form
    {
        private readonly INetHost _netHost;
        private readonly ISyncIOClient _client;

        public TaskManager(INetHost netHost, ISyncIOClient client)
        {
            InitializeComponent();

            _netHost = netHost;
            _client = client;

            _netHost.SetHandler<ProcessPacket>((c, p) => HandleProcessPacket(p));

            _client.Send(new ProcessPacket());
        }

        private void HandleProcessPacket(ProcessPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => HandleProcessPacket(packet)));
                return;
            }

            lvProcesses.BeginUpdate();
            foreach (var process in packet.Processes)
            {
                var l = lvProcesses.Items.Add(process.Name);
                l.Name = process.Id.ToString();
                l.SubItems.Add(process.Id.ToString());
                l.SubItems.Add(process.IsResponding ? "Running" : "Suspended");
                //l.SubItems.Add(process.Username);
                l.SubItems.Add((process.Memory / 1024).ToString("N0") + " KB");
                l.SubItems.Add(process.WindowTitle);
                l.SubItems.Add(process.FilePath);

                if (process.Id == packet.CurrentId)
                    l.ForeColor = Color.DodgerBlue;
                //if (suspendedProcessIds.Contains(pid))
                //    l.ForeColor = Color.Red;

                if (process.Icon != null)
                {
                    //imgListProcess.Images.Add(l.Text, Utils.BytesToImage(process.Icon));
                    l.ImageKey = l.Text;
                }
                else
                {
                    l.ImageIndex = 0;
                }
            }
            lvProcesses.EndUpdate();

            //sbpProcesses.Text = Format.Numeric(lvProcesses.Items.Count) + " Processes";
        }
    }
}