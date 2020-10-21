namespace SocketNet.UI.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using SyncIO;
    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.Plugins;
    using SyncIO.Server;
    using SyncIO.ServerPlugin;
    using SyncIO.Transport;
    using SyncIO.Transport.Compression.Defaults;
    using SyncIO.Transport.Encryption.Defaults;
    using SyncIO.Transport.Packets;
    using SyncIO.Transport.Packets.Internal;

    using SocketNet.Extensions;
    using SocketNet.UI.Extensions;

    // TODO: Keep list of all clients online and offline (database?)
    // TODO: Reliable reconnect
    // TODO: macOS needs brew install mono-libgdiplus
    // TODO: Statistics
    // TODO: Audit logs
    // TODO: Logs tab page (search by name, log type, etc)
    // TODO: Plugin compiler
    // TODO: Send plugins to client from host
    // TODO: Check OS if plugin is compatibile (property for each plugin Windows | MacOS | Linux)
    // TODO: Share host's screen
    // TODO: Custom named client
    // TODO: IApplication get paths
    // TODO: FileManager - Download/Upload
    // TODO: File Transfers list
    // TODO: Screen share specific monitors
    // TODO: macOS installer
    // TODO: Xamarin cross-platform task bar (plus comm between taskbar and agent/daemon/service, IPC?) Show client id and various info, close agent/daemon/service, other stuff maybe. Maybe no task tray/bar app :eyes:
    // TODO: Shared plugin library for common functions (screenshot, listview, etc)
    // TODO: Finish Port manager
    // TODO: Comment rest of code
    // TODO: Finish TestPlugin

    public partial class MainForm : Form, IUIHost, ILoggingHost
    {
        private const ushort Port = 9996;
        private const string PluginsPath = PluginManager<ISyncIOServerPlugin>.DefaultPluginFolderName;

        #region Variables

        private static readonly Packager _packer = new Packager(new Type[]
        {
            typeof(AuthPacket),
            typeof(ClientInfoPacket),
            typeof(PingPacket),
            typeof(ClientOptionPacket),
            typeof(StartFilePacket),
            typeof(FileDataPacket),
        });

        private TestServer _server;
        private Dictionary<Guid, ConnectedChatClient> _clients;
        private readonly PluginManager<ISyncIOServerPlugin> _pluginManager;
        private readonly byte[] Key = Packager.RandomBytes(32);// { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
        private readonly byte[] IV = Packager.RandomBytes(32);//{ 0x16, 0x15, 0x14, 0x13, 0x12, 0x11, 0x10, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
        private readonly System.Timers.Timer _pingTimer;

        //private readonly IUIHost _uiHost;
        private IUICallbacks _uiCallbacks;

        #endregion

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);

            if (_pluginManager.PluginCallbacks.ContainsKey(typeof(IUICallbacks)))
            {
                _uiCallbacks = (IUICallbacks)_pluginManager.PluginCallbacks[typeof(IUICallbacks)];
                _uiCallbacks?.OnInvalidated();
            }
        }

        #region Constructor

        public MainForm()
        {
            InitializeComponent();

            Setup();

            _pluginManager = new PluginManager<ISyncIOServerPlugin>
            (
                new Dictionary<Type, object>
                {
                    //{ typeof(IUIHost), _uiHost = this },
                    { typeof(IUIHost), this },
                    { typeof(INetHost), _server },
                    { typeof(ILoggingHost), this },
                },
                new Dictionary<Type, object>
                {
                    { typeof(IUICallbacks), _uiCallbacks ?? default }
                }
            );
            _pluginManager.PluginLoaded += (sender, e) =>
            {
                WriteLog(LogLevel.Info, $"Plugin {e.Plugin.PluginType.Name} loaded...");

                e.Plugin.PacketTypes.ForEach(x => _packer.AddType(x));
                e.Plugin.PluginType.OnPluginReady();
            };
            loadPluginsToolStripMenuItem.PerformClick();

            /*
            _pingTimer = new System.Timers.Timer(10 * 1000);
            _pingTimer.Elapsed += (sender, e) =>
            {
                foreach (var client in _server.Clients)
                {
                    client.Send(new PingPacket(DateTime.Now));
                }
            };
            _pingTimer.Start();
            */

            RefreshPorts();
        }

        #endregion

        #region Events

        private void Server_OnClientConnect(SyncIOServer sender, SyncIOConnectedClient client)
        {
            WriteLog(LogLevel.Info, $"[{client.Id}] New connection.");

            client.Send(new AuthPacket(Key, IV));

            WriteLog(LogLevel.Info, $"Enabling GZip compression...");
            client.SetCompression(new SyncIOCompressionGZip());

            WriteLog(LogLevel.Info, $"Enabling rijndael encryption using key {string.Join("", Key.Select(x => x.ToString("X2")))}");
            client.SetEncryption(new SyncIOEncryptionAes(Key, IV));

            AddClient(client);

            UpdateConnectionStatus();

            CallOnClientConnect(client);
        }

        private void Client_OnDisconnect(SyncIOConnectedClient client, Exception ex)
        {
            WriteLog(LogLevel.Info, $"[{client.Id}] Disconnected: {ex.Message}");

            RemoveClient(client);

            UpdateConnectionStatus();

            CallOnClientDisconnect(client);
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

            tabClients.Text = $"Clients ({_server.Clients.Count():N0})";
            lblConnectedClients.Text = _server.Clients.Count().ToString("N0");
            lblListeningPorts.Text = string.Join(", ", _server.ListeningPorts);
        }

        /*
        private void WriteLog(LogLevel type, string message, params object[] args)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => WriteLog(type, message, args)));
                return;
            }

            switch (type)
            {
                case LogLevel.Debug:
                    _loggingHost.Debug(message, args);
                    break;
                case LogLevel.Error:
                    _loggingHost.Error(message, args);
                    break;
                case LogLevel.Fatal:
                    _loggingHost.Fatal(message, args);
                    break;
                case LogLevel.Info:
                    _loggingHost.Info(message, args);
                    break;
                case LogLevel.Trace:
                    _loggingHost.Trace(message, args);
                    break;
                case LogLevel.Warning:
                    _loggingHost.Warn(message, args);
                    break;
            }
        }
        */

        #endregion

        #region Client Management

        private void AddClient(SyncIOConnectedClient client)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => AddClient(client)));
                return;
            }

            if (!lvClients.Items.ContainsKey(client.Id.ToString()))
            {
                //var c = _clients[client.ID];
                var l = lvClients.Items.Add(new ListViewItem(new string[]
                {
                    client.Id.ToString(),
                    client.EndPoint?.ToString(),
                    "--",
                    "--",
                    "--",
                    "--",
                    "--",
                    "--"
                })
                {
                    Name = client.Id.ToString(),
                    ImageKey = client.Id.ToString(),
                    Tag = client//_clients[client.ID]
                });

                //lvClients.GroupItem(l, client.GroupName);

                for (int i = 1; i < lvClients.Columns.Count; i++)
                {
                    l.SubItems.Add(string.Empty);
                }
            }

            client.OnDisconnect += Client_OnDisconnect;
        }

        private void AddDesktopImage(SyncIOConnectedClient client)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => AddDesktopImage(client)));
                return;
            }

            if (imgListDesktops.Images.ContainsKey(client.Id.ToString()))
            {
                imgListDesktops.Images.RemoveByKey(client.Id.ToString());
            }

            if (_clients.ContainsKey(client.Id))
            {
                imgListDesktops.Images.Add(client.Id.ToString(), _clients[client.Id].DesktopScreenshot);
            }
        }

        private void UpdateClient(SyncIOConnectedClient client)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => UpdateClient(client)));
                return;
            }

            if (!_clients.ContainsKey(client.Id))
                return;

            var c = _clients[client.Id];
            var l = lvClients.Items[client.Id.ToString()];
            if (l != null)
            {
                l.SubItems[lvClients.IndexFromKey("colUserPC")].Text = $"{c.UserName} / {c.MachineName}";
                l.SubItems[lvClients.IndexFromKey("colOS")].Text = $"{c.OS} ({c.OsPlatform}, {c.OsArchitecture})";
                l.SubItems[lvClients.IndexFromKey("colCPU")].Text = c.ProcessorName;
                l.SubItems[lvClients.IndexFromKey("colRAM")].Text = c.TotalMemory.ToBytes();
                l.SubItems[lvClients.IndexFromKey("colVersion")].Text = c.ClientVersion?.ToString();
                l.SubItems[lvClients.IndexFromKey("colUptime")].Text = c.Uptime.ToString();
                l.SubItems[lvClients.IndexFromKey("colLastUpdated")].Text = c.LastUpdated.ToString();
                //l.Tag = _clients[client.Id];
            }

            lvClients.GroupItem(l, c.GroupName);

            lvClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            //lvClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            AddDesktopImage(client);

        }

        private void RemoveClient(SyncIOConnectedClient client)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => RemoveClient(client)));
                return;
            }

            if (_clients.ContainsKey(client.Id))
            {
                _clients.Remove(client.Id);
            }

            if (lvClients.Items.ContainsKey(client.Id.ToString()))
            {
                lvClients.Items.RemoveByKey(client.Id.ToString());
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
                new ColumnHeader { Name = "colOS", Text = "OS (Platform)", DisplayIndex = 3 },
                new ColumnHeader { Name = "colCPU", Text = "CPU", DisplayIndex = 4 },
                new ColumnHeader { Name = "colRAM", Text = "RAM", DisplayIndex = 5 },
                new ColumnHeader { Name = "colVersion", Text = "Version", DisplayIndex = 6 },
                new ColumnHeader { Name = "colUptime", Text = "Uptime", DisplayIndex = 7 },
                new ColumnHeader { Name = "colLastUpdated", Text = "Last Updated", DisplayIndex = 8 },
            }.ToArray());

            _clients = new Dictionary<Guid, ConnectedChatClient>();
            _server = new TestServer(TransportProtocol.IPv4, _packer);
            _server.OnClientConnect += Server_OnClientConnect;

            SetupRpcs();
            SetupPacketHandlers();

            _server.ListenTcp(Port);

            if (!_server.Any())
            {
                Console.WriteLine("Failed to listen on any ports.");
                Application.Exit();
            }

            Text = "Listening on ";
            foreach (var sock in _server)
            {
                Text += sock.EndPoint.Port.ToString() + ", ";
                sock.OnClose += (sender, err) => WriteLog(LogLevel.Info, $"[{sender.EndPoint.Port}] Listening socket closed. {err}");
            }

            UpdateConnectionStatus();

            PopulateGroupByList();
        }

        private void SetupRpcs()
        {
            var sayHi = _server.RegisterRemoteFunction("say", new Func<string, string>((string msg) =>
            {
                WriteLog(LogLevel.Debug, $"Say function called: {msg}");
                return msg;
            }));

            var getTime = _server.RegisterRemoteFunction("GetTime", new Func<string>(() =>
            {
                WriteLog(LogLevel.Debug, "Time function called");
                return string.Format("It is {0}.", DateTime.Now.ToShortTimeString());
            }));
            getTime.SetAuthFunc((c, f) =>
            {
                return _clients[c.Id].CanUseTimeCommand;
            });

            var toggleTime = _server.RegisterRemoteFunction("toggletime", new Func<string>(() => "\"time\" command has been toggled."));
            toggleTime.SetAuthFunc((c, f) =>
            {
                return _clients[c.Id].CanUseTimeCommand = !_clients[c.Id].CanUseTimeCommand;
            });
        }

        private void SetupPacketHandlers()
        {
            /*
            var sendToAll = new Action<IPacket>((p) =>
            {
                // Send to all clients who have set a name.
                foreach (var c in _clients.Select(x => x.Value))
                    c.Connection.Send(p);
            });
            */

            _server.SetHandler<IPacket>(CallOnPacketReceived);
            _server.SetHandler<ClientInfoPacket>((c, p) =>
            {
                WriteLog(LogLevel.Debug, $"[{c.Id}] ClientInfo received...");
                if (_clients.ContainsKey(c.Id))
                    _clients[c.Id].Update(p);
                else
                    _clients.Add(c.Id, new ConnectedChatClient(c, p));
                c.Tag = _clients[c.Id];
                UpdateClient(c);
            });
            _server.SetHandler<PingPacket>((c, p) =>
            {
                if (_clients.ContainsKey(c.Id))
                {
                    WriteLog(LogLevel.Debug, $"[{c.Id}] PingPacket latency {Convert.ToInt32(p.Latency)}ms...");
                    _clients[c.Id].Latency = Convert.ToInt32(p.Latency);
                }
            });
            _server.SetHandler<StartFilePacket>((c, p) =>
            {
            });
            _server.SetHandler<FileDataPacket>((c, p) =>
            {
                // TODO: Use FileManager for transfers in new thread
                var userFolder = Path.Combine("Files", c.EndPoint.Address.ToString());
                if (!Directory.Exists(userFolder))
                {
                    Directory.CreateDirectory(userFolder);
                }
                var path = Path.Combine(userFolder, Path.GetFileName(p.FilePath));
                using (var fs = File.OpenWrite(path))
                {
                    fs.Position = p.Offset;
                    //Write the bytes to the stream.
                    fs.Write(p.Block, 0, p.Block.Length);
                }
            });
        }

        #endregion

        #region Plugin Host Handlers

        private void CallOnPacketReceived(SyncIOConnectedClient client, IPacket packet)
        {
            foreach (var plugin in _pluginManager.Plugins)
            {
                plugin.Value.PluginType.OnPacketReceived(client, packet);
            }
        }

        private void CallOnClientConnect(SyncIOConnectedClient client)
        {
            foreach (var plugin in _pluginManager.Plugins)
            {
                plugin.Value.PluginType.OnClientConnect(client);
            }
        }

        private void CallOnClientDisconnect(SyncIOConnectedClient client)
        {
            foreach (var plugin in _pluginManager.Plugins)
            {
                CloseOpenForms(client);
                plugin.Value.PluginType.OnClientDisconnect(client);
            }
        }

        #endregion

        #region Plugins ContextMenuStrip

        private void LoadPluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _pluginManager.LoadPlugins(PluginsPath);

            lvPlugins.Items.Clear();
            foreach (var plugin in _pluginManager.Plugins)
            {
                var l = new ListViewItem(new string[]
                {
                    plugin.Key.ToString(),
                    plugin.Value.PluginType.Name,
                    plugin.Value.PluginType.Author,
                    plugin.Value.Version.ToString()
                })
                {
                    Name = plugin.Key.ToString(),
                    Tag = plugin
                };
                lvPlugins.Items.Add(l);
            }

            tabPlugins.Text = $"Plugins ({_pluginManager.Plugins.Count})";
        }

        #endregion

        #region Plugin UI

        public void AddContextMenuEntry(ContextEntry item)
        {
            if (cmsClients.Items.ContainsKey(item.Name))
                return;

            var tsmi = (ToolStripMenuItem)cmsClients.Items.Add(item.Name, string.IsNullOrEmpty(item.IconPath) ? null : Image.FromFile(item.IconPath), (sender, e) => item.OnClick?.Invoke(this, GetSelectedClients()));
            tsmi.Tag = item;
            AddChildContextMenuItem(tsmi, item.Children);
        }

        private void AddChildContextMenuItem(ToolStripMenuItem parent, List<ContextEntry> children)
        {
            if (children == null || children.Count == 0)
                return;

            foreach (var child in children)
            {
                if (!parent.DropDown.Items.ContainsKey(child.Name))
                {
                    parent.DropDown.Items.Add(child.Name, string.IsNullOrEmpty(child.IconPath) ? null : Image.FromFile(child.IconPath), (sender, e) =>
                    {
                        if (!(sender is ToolStripMenuItem tsmi))
                            return;

                        if (!(tsmi.Tag is ContextEntry contextMenuEntry))
                            return;

                        contextMenuEntry.OnClick?.Invoke(this, GetSelectedClients());
                    }).Tag = child;
                }
            }
        }

        public void AddColumnEntry(ColumnEntry item)
        {
            if (lvClients.Columns.ContainsKey(item.Text))
                return;

            var column = lvClients.Columns.Add(item.Text, item.Text, item.Width, HorizontalAlignment.Left, string.Empty);
            foreach (ListViewItem l in lvClients.Items)
            {
                l.SubItems.Add("N/A");
            }

            column.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private delegate void UpdateColumnEventHandler(Guid clientId, ColumnEntry item, string text);

        public void SetColumnValue(Guid clientId, ColumnEntry item, string text)
        {
            if (lvClients.InvokeRequired)
            {
                lvClients.Invoke(new UpdateColumnEventHandler(SetColumnValue), clientId, item, text);
                return;
            }

            if (item == null)
                return;

            if (!lvClients.Columns.ContainsKey(item.Text))
                return;

            try
            {
                foreach (ListViewItem l in lvClients.Items)
                {
                    if (!(l.Tag is SyncIOConnectedClient client))
                        continue;

                    if (client.Id != clientId)
                        continue;

                    var col = lvClients.Columns[item.Text];
                    if (col == null)
                        continue;

                    l.SubItems[col.Index].Text = text;
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogLevel.Error, $"SetColumnValue: {ex}");
            }
        }

        private Dictionary<Guid, ISyncIOClient> GetSelectedClients()
        {
            var dict = new Dictionary<Guid, ISyncIOClient>();
            foreach (ListViewItem l in lvClients.SelectedItems)
            {
                if (!(l.Tag is SyncIOConnectedClient client))
                    continue;

                dict.Add(client.Id, client);
            }

            return dict;
        }

        #endregion

        #region ContextMenuStrip

        #region View

        private void DetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvClients.View = View.Details;
            detailsToolStripMenuItem.Checked = true;
            tileToolStripMenuItem.Checked = false;
        }

        private void TileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvClients.View = View.Tile;
            detailsToolStripMenuItem.Checked = false;
            tileToolStripMenuItem.Checked = true;
        }

        #endregion

        #region Select

        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lvClients.Items.Count; i++)
            {
                lvClients.Items[i].Selected = true;
            }
        }

        private void SelectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lvClients.Items.Count; i++)
            {
                lvClients.Items[i].Selected = false;
            }
        }

        #endregion

        #region Group Manager

        private void GroupManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvClients.Items.Count <= 0)
                return;

            //if (FormExtensions.IsFormOpen("Group Manager"))
            //    return;

            var gm = new GroupManager(lvClients)
            {
                //Icon = Icon.FromHandle(new Bitmap(groupManagerToolStripMenuItem.Image).GetHicon())
            };
            gm.FormClosing += (s, ee) => LoadGroups();
            gm.Show();
        }

        #region Group By

        private void ResetGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem l in lvClients.SelectedItems)
            {
                if (!(l.Tag is SyncIOConnectedClient client))
                    continue;

                var c = _clients[client.Id];
                lvClients.GroupItem(l, c.GroupName);
            }
        }

        private void GroupByToolStripMenu_Click(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            GroupClients(lvClients.ColumnIndex(tsmi.Name), true);
        }

        private void PopulateGroupByList()
        {
            groupByToolStripMenuItem.DropDownItems.Clear();
            groupByToolStripMenuItem.DropDownItems.Add("Reset Group", null, ResetGroupToolStripMenuItem_Click).Name = "Reset Group";
            groupByToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            foreach (ColumnHeader col in lvClients.Columns)
            {
                groupByToolStripMenuItem.DropDownItems.Add(col.Text, null, GroupByToolStripMenu_Click).Name = "col" + col.Text;
            }
        }

        #endregion

        #endregion

        private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvClients.SelectedItems)
            {
                if (!(lvi.Tag is SyncIOConnectedClient client))
                    continue;

                client.Send(new ClientInfoPacket());
            }
        }

        #region Client

        private void DisconnectToolStripMenuItem_Click(object sender, EventArgs e) => SendControlPacket(ClientOptions.Disconnect);

        private void ReconnectToolStripMenuItem_Click(object sender, EventArgs e) => SendControlPacket(ClientOptions.Reconnect);

        private void RestartApplicationToolStripMenuItem_Click(object sender, EventArgs e) => SendControlPacket(ClientOptions.RestartApp);

        private void CloseApplicationToolStripMenuItem_Click(object sender, EventArgs e) => SendControlPacket(ClientOptions.CloseApp);

        #endregion

        #endregion

        #region Group Manager

        private void LoadGroups()
        {
            switchToGroupToolStripMenuItem.DropDownItems.Clear();
            switchToGroupToolStripMenuItem.DropDownItems.Add("Default Group", /*imageList1.Images[0]*/null, tsmi_Click).Name = "Default Group";
            switchToGroupToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

            foreach (ListViewGroup g in lvClients.Groups)
            {
                if (string.Compare(g.Header, "Default Group", true) != 0 &&
                    string.Compare(g.Header, LvGroupExtensions.DefaultGroup, true) != 0)
                {
                    if (!switchToGroupToolStripMenuItem.DropDownItems.ContainsKey(g.Header))
                    {
                        switchToGroupToolStripMenuItem.DropDownItems.Add(g.Header, /*imageList1.Images[1]*/null, tsmi_Click).Name = g.Header;
                    }
                }
            }
        }

        private void tsmi_Click(object sender, EventArgs e)
        {
            if (lvClients.SelectedItems.Count <= 0)
                return;

            var clicked = sender as ToolStripMenuItem;
            var groupName = string.Compare(clicked.Text, "Default Group", true) == 0
                ? LvGroupExtensions.DefaultGroup
                : clicked.Text;
            foreach (ListViewItem l in lvClients.SelectedItems)
            {
                lvClients.GroupItem(l, groupName);
            }
        }

        private void lvClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: 
            //Utils.AssignContextMenuStripToListView(cmsFeatures, lvClients);
            LoadGroups();
        }

        private void GroupClients(int index, bool selected)
        {
            foreach (ListViewItem l in lvClients.Items)
            {
                if ((selected && l.Selected) || !selected)
                {
                    l.Group.Items.Remove(l);
                    lvClients.GroupItem(l, l.SubItems[index].Text);
                }
            }
        }

        #endregion

        private void SendControlPacket(ClientOptions option)
        {
            foreach (ListViewItem lvi in lvClients.SelectedItems)
            {
                if (!(lvi.Tag is SyncIOConnectedClient client))
                    continue;

                client.Send(c => c.Disconnect(new Exception()), new ClientOptionPacket(option));
            }
        }

        public void CloseOpenForms(ISyncIOClient client)
        {
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

        public void RefreshPorts()
        {
            lvPorts.Items.Clear();

            /*
            while (iSQLReader.NextRecord() != false)
            {
                var port = Convert.ToUInt16(iSQLReader["port"].ToString());
                var enabled = Convert.ToBoolean(Convert.ToUInt16(iSQLReader["enabled"]));
                AddListener(port, enabled);
            }
            */

            //Globals.MainForm.listeningPortsToolStripMenuItem.DropDownItems.Clear();

            foreach (var port in _server.ListeningPorts)
            {
                //var server = _server[port] as SyncIOServer;
                //var l = lvPorts.Items.Add($"Port: {port}");
                //l.SubItems.Add($"Connections: {server.Clients.Count()}");
                //l.SubItems.Add("State: " + (server..Listening ? "Listening" : "Idle"));
                //l.ImageIndex = Convert.ToInt32(listener.Listening);

                /*
                Globals.MainForm.listeningPortsToolStripMenuItem.DropDownItems.Add
                (
                    "Port " + listener.Port + " - " + listener.Clients.Length + " Connections",
                    imgListPorts.Images[Convert.ToInt32(listener.Listening)]
                );
                */
            }

            //Globals.MainForm.lblStatus.Image = Globals.MainForm.IsListening() ? Properties.Resources.status_online : Properties.Resources.status_offline;
        }

        internal TestServer PortMgr_PortAdded(ushort port, bool listen)
        {
            /*
            foreach (var l in Globals.Listeners)
            {
                if (l.Port == port)
                    return l;
            }
            */

            var listener = new TestServer(TransportProtocol.IPv4, _packer);// port);
            //listener.SocketAccepted += l_SocketAccepted;
            //listener.StateChanged += l_StateChanged;
            //listener.KeepAlive = true;
            //listener.BackLog = 0x7FFFFFFF; //int.MaxValue;
            //listener.NoDelay = true;

            if (listen)
            {
                var socket = listener.ListenTcp(port);
            }

            /*
            if (!Globals.Listeners.Contains(listener))
            {
                Globals.Listeners.Add(listener);

                foreach (var plugin in _pluginManager.Plugins)
                {
                    plugin.Value.ClientHandlers.ListenerAddedRemoved(listener, true);
                }
            }

            Globals.SQL.AddPort(port, listener.Connections, listen);

            UpdatePortsCount();
            */

            return listener;
        }

        #region Logging Host

        public void Trace(string format, params object[] args)
        {
            WriteLog(LogLevel.Trace, format, args);
        }

        public void Debug(string format, params object[] args)
        {
            WriteLog(LogLevel.Debug, format, args);
        }

        public void Error(string format, params object[] args)
        {
            WriteLog(LogLevel.Error, format, args);
        }

        public void Error(Exception error)
        {
            WriteLog(LogLevel.Error, error.ToString());
        }

        public void Fatal(string format, params object[] args)
        {
            WriteLog(LogLevel.Fatal, format, args);
        }

        public void Fatal(Exception error)
        {
            WriteLog(LogLevel.Fatal, error.ToString());
        }

        public void Info(string format, params object[] args)
        {
            WriteLog(LogLevel.Info, format, args);
        }

        public void Warn(string format, params object[] args)
        {
            WriteLog(LogLevel.Warning, format, args);
        }

        public void WriteLog(LogLevel type, string machine, string message, params object[] args)
        {
            // TODO: Include machine
            WriteLog(type, message, args);
        }

        private void WriteLog(LogLevel type, string message, params object[] args)
        {
            if (lvLogs.InvokeRequired)
            {
                lvLogs.Invoke(new MethodInvoker(() => WriteLog(type, message, args)));
                return;
            }

            var l = lvLogs.Items.Add(DateTime.Now.ToString());
            l.SubItems.Add(Environment.MachineName);
            l.SubItems.Add(type.ToString().ToUpper());
            l.SubItems.Add(string.Format(message, args));

            switch (type)
            {
                case LogLevel.Debug:
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    l.BackColor = Color.Red;
                    l.ForeColor = Color.White;
                    break;
                case LogLevel.Info:
                    break;
                case LogLevel.Warning:
                    l.BackColor = Color.Yellow;
                    l.ForeColor = Color.Black;
                    break;
            }

            tabLogs.Text = $"Logs ({lvLogs.Items.Count})";

            //_rtb.AppendText(DateTime.Now.ToString());
            //_rtb.AppendText(" [");
            //_rtb.AppendText(type.ToString().ToUpper());
            //_rtb.AppendText("] ");
            //_rtb.AppendText(args.Length > 0 ? string.Format(message, args) : message);
            //_rtb.AppendText(Environment.NewLine);
        }

        #endregion
    }

    public class TestServer : SyncIOServer, INetHost
    {
        public TestServer(TransportProtocol protocol, Packager packager) 
            : base(protocol, packager)
        {
        }
    }
}