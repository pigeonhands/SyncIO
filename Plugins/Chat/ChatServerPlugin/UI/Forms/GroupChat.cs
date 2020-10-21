namespace ChatServerPlugin.UI.Forms
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    using SyncIO.Network;
    using SyncIO.ServerPlugin;

    public partial class GroupChat : Form
    {
        const int ERROR = -1;

        #region Variables

        private readonly INetHost _netHost;
        private readonly ISyncIOClient _client;
        private ISyncIOClient[] _selectedClients = null;

        #endregion

        #region Constructor

        public GroupChat(INetHost netHost, ISyncIOClient[] selectedClients)
        {
            InitializeComponent();

            _netHost = netHost;
            _selectedClients = selectedClients;

            //CreateChatLogsTable();
        }

        #endregion

        /*
        public void CreateChatLogsTable()
        {
            try
            {
                if (!Globals.SQL.DatabaseExists(db))
                {
                    Globals.SQL.CreateDatabase(db);
                }
                string sql = @"
CREATE TABLE IF NOT EXISTS `logs` 
(
	`id`	   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`data`     TEXT    NOT NULL,
    `dateTime` TEXT    NOT NULL
);";
                bool bResult = Globals.SQL.ExecuteNonQuery(db, sql) != ERROR;
            }
            catch (Exception ex)
            {
                Globals.Logger.LogException("CreateChatLogsTable", ex);
            }
        }
        public ISqlReader LoadGroupChatLogs()
        {
            return Globals.SQL.ExecuteQuery(db, "SELECT * FROM logs");
        }
        public void AddChatLog(string data)
        {
            bool bResult = Globals.SQL.ExecuteNonQuery
            (
                db,
                Globals.SQL.FormatQuery
                (
                    "INSERT INTO `logs` (data, dateTime) VALUES ('{0}', '{1}')",
                    data,
                    DateTime.Now.ToString()
                )
            ) != ERROR;
        }
        */

        #region Form, Control Events

        private void Form_Load(object sender, EventArgs e)
        {
            foreach (var client in _selectedClients)
            {
                var l = lvClients.Items.Add(client.Id.ToString());
                l.SubItems.Add(client.EndPoint.ToString());
                l.Checked = true;
                l.Tag = client;
            }

            lblTotalClients.Text = "Total Clients: " + _selectedClients.Length.ToString("N0");

            SendStartGroupChat(_selectedClients);
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;

            if (chkSaveGroupChatLogs.Checked)
            {
                if (rtbChat.Text.Length > 0 && !string.IsNullOrEmpty(rtbChat.Text))
                {
                    // TODO: AddChatLog(rtbChat.Text);
                }
            }

            // TODO: SendStopGroupChat(Globals.Network.Clients);
            e.Cancel = false;
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && chkSendOnEnter.Checked)
            {
                SendGroupMessage();
            }
        }

        private void rtbChat_TextChanged(object sender, EventArgs e)
        {
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.ScrollToCaret();
        }

        private void btnSend_Click(object sender, EventArgs e) => SendGroupMessage();

        private void btnNudge_Click(object sender, EventArgs e) => SendNudge();

        private void chkAllowFormClose_CheckedChanged(object sender, EventArgs e)
        {
            if (lvClients.CheckedItems.Count == 0) return;

            foreach (ListViewItem l in lvClients.CheckedItems)
            {
                var c = l.Tag as ISyncIOClient;
                /*
                Globals.Network.Send(c,
                    (byte)MainPacket.GroupChatPacket,
                    (byte)GroupChatPacket.ChatSettings,
                    (byte)MainPacket.Null,
                    (byte)MainPacket.Null,
                    chkAllowFormClose.Checked
                );
                */
            }
        }

        #endregion

        #region Packet Senders

        private void SendStartGroupChat(ISyncIOClient[] clients)
        {
            rtbChat.Clear();
            foreach (var client in clients)
            {
                /*
                Globals.Network.Send(client,
                    (byte)MainPacket.GroupChatPacket,
                    (byte)GroupChatPacket.StartChat,
                    (byte)MainPacket.Null,
                    (byte)MainPacket.Null,
                    chkAllowFormClose.Checked
                );
                */
            }
        }
        private void SendStopGroupChat(ISyncIOClient[] clients)
        {
            rtbChat.Clear();
            foreach (var client in clients)
            {
                /*
                Globals.Network.Send(client,
                    (byte)MainPacket.GroupChatPacket,
                    (byte)GroupChatPacket.StopChat,
                    (byte)MainPacket.Null,
                    (byte)MainPacket.Null
                );
                */
            }
        }
        private void SendGroupMessage()
        {
            if (lvClients.CheckedItems.Count == 0)
                return;

            if (string.IsNullOrEmpty(txtMessage.Text))
                return;

            foreach (ListViewItem l in lvClients.CheckedItems)
            {
                var c = l.Tag as ISyncIOClient;
                /*
                Globals.Network.Send(c,
                    (byte)MainPacket.GroupChatPacket,
                    (byte)GroupChatPacket.ChatText,
                    (byte)MainPacket.Null,
                    (byte)MainPacket.Null,
                    txtMessage.Text
                );
                */
            }

            rtbChat.AppendText(string.Format("[{0}] You: {1}{2}{3}",
                DateTime.Now.ToString("hh:mm:ss tt"),
                txtMessage.Text,
                Environment.NewLine,
                Environment.NewLine));
            txtMessage.Clear();
        }
        private void SendNudge()
        {
            if (lvClients.CheckedItems.Count == 0)
                return;

            foreach (ListViewItem l in lvClients.CheckedItems)
            {
                var c = l.Tag as ISyncIOClient;
                /*
                Globals.Network.Send(c,
                    (byte)MainPacket.GroupChatPacket,
                    (byte)GroupChatPacket.Nudge,
                    (byte)MainPacket.Null,
                    (byte)MainPacket.Null
                );
                */

                rtbChat.AppendText(string.Format("[{0}] *** You sent a nudge to {1} ***\n\n", DateTime.Now.ToString("hh:mm:ss tt"), c.Id.ToString() + " (" + c.EndPoint + ")"));
            }
        }

        #endregion

        #region Packet Handlers

        delegate void GroupMessageEventHandler(ISyncIOClient client, object o);
        public void HandleGroupMessage(ISyncIOClient client, object o)
        {
            string data = (string)o;

            //Globals.UI.SetColumnValue(client, Globals.GroupChatColumn, data);

            rtbChat.AppendText(string.Format("[{0}] {1} ({2}): {3}\n\n",
                DateTime.Now.ToString("hh:mm:ss tt"),
                client.Id.ToString(),
                client.EndPoint.ToString(),
                data));
        }

        delegate void ChatClientStoppedEventHandler(ISyncIOClient client);
        public void HandleChatClientStopped(ISyncIOClient client)
        {
            foreach (ListViewItem l in lvClients.Items)
            {
                var c = l.Tag as ISyncIOClient;
                if (client.EndPoint == c.EndPoint)
                {
                    l.Remove();

                    rtbChat.AppendText(string.Format("[{0}] {1} ({2}): Client has disconnected from group chat.\n\n",
                        DateTime.Now.ToString("hh:mm:ss tt"),
                        client.Id.ToString(),
                        client.EndPoint.ToString()));
                }
            }
        }

        #endregion

        private void ExportChatLogs()
        {
            using (SaveFileDialog s = new SaveFileDialog())
            {
                s.FilterIndex = 1;
                s.Filter = "Text Documents *.txt|*.txt";
                s.DefaultExt = "*.txt|*.txt";
                if (s.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(s.FileName, rtbChat.Text);
                }
            }
        }

        private void btnChatLogsArchive_Click(object sender, EventArgs e)
        {
            //if (!Utils.IsFormOpen("Group Chat Logs Archive"))
            //{
            //    new ChatLogsArchive().Show();
            //}
        }
    }
}