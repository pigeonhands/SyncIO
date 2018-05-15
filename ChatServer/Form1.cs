namespace ChatServer
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.Server;
    using SyncIO.Transport;
    using SyncIO.Transport.Compression.Defaults;
    using SyncIO.Transport.Encryption.Defaults;
    using SyncIO.Transport.Packets;

    using ChatServer.UI.Extensions;

    public partial class Form1 : Form
    {
        const       ushort Port = 9996;
        readonly    byte[] Key  = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };
        readonly    byte[] IV   = { 0xF, 0xE, 0xD, 0xC, 0xB, 0xA, 0x9, 0x8, 0x7, 0x6, 0x5, 0x4, 0x3, 0x2, 0x1, 0x0 };

        static readonly Packager _packer = new Packager(new Type[]
        {
            typeof(ClientInfo),
            typeof(ChatMessage),
            typeof(FilePacket),
            typeof(DesktopScreenshot)
        });
        private SyncIOServer _server;
        private Dictionary<Guid, ConnectedChatClient> _clients;

        public Form1()
        {
            InitializeComponent();
            Setup();
        }

        #region Events

        private void Server_OnClientConnect(SyncIOServer sender, SyncIOConnectedClient client)
        {
            WriteLog(LogType.Info, $"[{client.ID}] New connection.");

            Console.WriteLine($"Enabling GZip compression...");
            client.SetCompression(new SyncIOCompressionGZip());

            Console.WriteLine($"Enabling rijndael encryption using key {string.Join("", Key.Select(x => x.ToString("X2")))}");
            client.SetEncryption(new SyncIOEncryptionAes(Key, IV));

            AddClient(client);

            UpdateConnectionStatus();
        }

        private void Client_OnDisconnect(SyncIOConnectedClient client, Exception ex)
        {
            WriteLog(LogType.Info, $"[{client.ID}] Disconnected: {ex.Message}");

            RemoveClient(client);

            UpdateConnectionStatus();
        }

        #endregion

        #region Update UI

        private void UpdateConnectionStatus()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(UpdateConnectionStatus));
                return;
            }

            lblConnectedClients.Text = _server.Clients.Count().ToString("N0");
            lblListeningPorts.Text = string.Join(", ", _server.ListeningPorts);
        }

        private void WriteLog(LogType type, string message, params object[] args)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => WriteLog(type, message, args)));
                return;
            }

            rtbLogs.AppendText(DateTime.Now.ToString());
            rtbLogs.AppendText($" [{type.ToString().ToUpper()}] ");
            rtbLogs.AppendText(string.Format(message, args));
            rtbLogs.AppendText(Environment.NewLine);
        }

        #endregion

        #region Client Management

        private void AddClient(SyncIOConnectedClient client)
        {
            if (InvokeRequired)
            {
                Invoke(new ClientManagementEventHandler(AddClient), client);
                return;
            }

            if (!lvClients.Items.ContainsKey(client.ID.ToString()))
            {
                //var c = _clients[client.ID];
                lvClients.Items.Add(new ListViewItem(new string[]
                {
                    client.ID.ToString(),
                    client.EndPoint.ToString(),
                    "--",
                    "--",
                    "--"
                })
                {
                    Name = client.ID.ToString(),
                    ImageKey = client.ID.ToString(),
                    Tag = client//_clients[client.ID]
                });
            }

            client.OnDisconnect += Client_OnDisconnect;

            client.Send(new DesktopScreenshot(null));
        }

        private void UpdateClient(SyncIOConnectedClient client)
        {
            if (InvokeRequired)
            {
                Invoke(new ClientManagementEventHandler(UpdateClient), client);
                return;
            }

            if (!_clients.ContainsKey(client.ID))
                return;

            var c = _clients[client.ID];
            var l = lvClients.Items[client.ID.ToString()];
            if (l != null)
            {
                l.SubItems[lvClients.IndexFromKey("colUserPC")].Text = $"{c.UserName?.ToString()} / {c.MachineName?.ToString()}";
                l.SubItems[lvClients.IndexFromKey("colOS")].Text = c.OS?.ToString();
                l.SubItems[lvClients.IndexFromKey("colVersion")].Text = c.ClientVersion?.ToString();
                l.Tag = _clients[client.ID];
            }

            lvClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            //lvClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

        }

        private void RemoveClient(SyncIOConnectedClient client)
        {
            if (InvokeRequired)
            {
                Invoke(new ClientManagementEventHandler(RemoveClient), client);
                return;
            }

            

            if (_clients.ContainsKey(client.ID))
            {
                _clients.Remove(client.ID);
            }

            if (lvClients.Items.ContainsKey(client.ID.ToString()))
            {
                lvClients.Items.RemoveByKey(client.ID.ToString());
            }
        }

        #endregion

        #region Server Setup

        private void Setup()
        {
            lvClients.Columns.AddRange(new List<ColumnHeader>
            {
                new ColumnHeader { Name = "colId", Text = "ID", DisplayIndex = 0 },
                new ColumnHeader { Name = "colEndPoint", Text = "EndPoint", DisplayIndex = 1 },
                new ColumnHeader { Name = "colUserPC", Text = "User / Computer", DisplayIndex = 2 },
                new ColumnHeader { Name = "colOS", Text = "OS", DisplayIndex = 3 },
                new ColumnHeader { Name = "colVersion", Text = "Version", DisplayIndex = 4 },
            }.ToArray());

            _clients = new Dictionary<Guid, ConnectedChatClient>();
            _server = new SyncIOServer(TransportProtocol.IPv4, _packer);
            _server.OnClientConnect += Server_OnClientConnect;

            SetupRpcs();
            SetupPacketHandlers();

            _server.ListenTCP(Port);

            if (!_server.Any())
            {
                Console.WriteLine("Failed to listen on any ports.");
                Application.Exit();
            }

            Text = "Listening on ";
            foreach (var sock in _server)
            {
                Text += sock.EndPoint.Port.ToString();
                sock.OnClose += (sender, err) => WriteLog(LogType.Info, $"[{sender.EndPoint.Port}] Listening socket closed. {err}");
            }

            UpdateConnectionStatus();
        }

        private void SetupRpcs()
        {
            var sayHi = _server.RegisterRemoteFunction("say", new Func<string, string>((string msg) =>
            {
                WriteLog(LogType.Debug, $"Say function called: {msg}");
                return msg;
            }));

            var getTime = _server.RegisterRemoteFunction("GetTime", new Func<string>(() =>
            {
                WriteLog(LogType.Debug, "Time function called");
                return string.Format("It is {0}.", DateTime.Now.ToShortTimeString());
            }));
            getTime.SetAuthFunc((c, f) =>
            {
                return _clients[c.ID].CanUseTimeCommand;
            });

            var toggleTime = _server.RegisterRemoteFunction("toggletime", new Func<string>(() => "\"time\" command has been toggled."));
            toggleTime.SetAuthFunc((c, f) =>
            {
                return (_clients[c.ID].CanUseTimeCommand = !_clients[c.ID].CanUseTimeCommand);
            });
        }

        private void SetupPacketHandlers()
        {
            var sendToAll = new Action<IPacket>((p) =>
            {
                //Send to all clients who have set a name.
                foreach (var c in _clients.Select(x => x.Value))
                    c.Connection.Send(p);
            });

            _server.SetHandler<ClientInfo>((c, p) =>
            {
                sendToAll(new ChatMessage($"{p.UserName} connected. ({c.ID})"));
                _clients.Add(c.ID, new ConnectedChatClient(c, p.OS, p.UserName, p.MachineName, p.ClientVersion));
            });
            _server.SetHandler<ChatMessage>((c, p) =>
            {
                var msg = $"<{_clients[c.ID].UserName}> {p.Message}";
                sendToAll(new ChatMessage(msg));
                WriteLog(LogType.Info, msg);
            });
            _server.SetHandler<FilePacket>((c, p) =>
            {
                WriteLog(LogType.Debug, $"[{c.ID}] File {p.FileName} received...");
                for (int i = 0; i < p.Bytes.Count; i++)
                {
                    WriteLog(LogType.Debug, $"Writing index file {i} of {p.FileName}...");
                    var ext = Path.GetExtension(p.FileName);
                    File.WriteAllBytes(p.FileName.Replace(ext, i + ext), p.Bytes[i]);
                }
            });
            _server.SetHandler<DesktopScreenshot>((c, p) =>
            {
                WriteLog(LogType.Debug, $"[{c.ID}] Desktop screenshot received...");
                if (_clients.ContainsKey(c.ID))
                {
                    _clients[c.ID].DesktopScreenshot = ByteArrayToImage(p.Image);
                    AddDesktopImage(c);
                    UpdateClient(c);
                }
            });
        }

        #endregion

        private void AddDesktopImage(SyncIOConnectedClient client)
        {
            if (InvokeRequired)
            {
                Invoke(new ClientManagementEventHandler(AddDesktopImage), client);
                return;
            }

            if (imageList1.Images.ContainsKey(client.ID.ToString()))
            {
                imageList1.Images.RemoveByKey(client.ID.ToString());
            }

            if (_clients.ContainsKey(client.ID))
            {
                imageList1.Images.Add(client.ID.ToString(), _clients[client.ID].DesktopScreenshot);
            }
        }

        delegate void ClientManagementEventHandler(SyncIOConnectedClient client);

        public Image ByteArrayToImage(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }
    }

    public enum LogType
    {
        Info,
        Debug,
        Error,
        Warning
    }
}