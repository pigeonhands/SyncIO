namespace FileManagerServerPlugin.UI.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    using FileInfo = SyncIO.Common.Packets.FileInfo;
    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.ServerPlugin;

    using FileManagerServerPlugin.Extensions;
    using FileManagerServerPlugin.UI.Dialogs;
    using SyncIO.Transport.Packets.Internal;

    public partial class FileManager : Form
    {
        private readonly INetHost _netHost;
        private readonly ISyncIOClient _client;
        private readonly List<string> _previousPlaces;
        private readonly List<FileInfo> _fileList;
        private readonly List<string> _validFolderTypes = new List<string> { "hdd", "usb", "net", "cd", "dir" };
        private string _machineName;
        private char _directorySeparator;

        public FileManager(INetHost netHost, ISyncIOClient client)
        {
            InitializeComponent();

            _netHost = netHost;
            _client = client;
            _previousPlaces = new List<string>();
            _fileList = new List<FileInfo>();

            RefreshImageListImages(imgListTypes);

            _netHost.SetHandler<DrivePlacesPacket>((c, p) => HandleDrivePlacesPacket(p));
            _netHost.SetHandler<FilePacket>((c, p) => HandleFilesFolders(p.Files));

            // Fetch list of available drives
            SendGetExplorerPlaces();
        }

        #region Form UI Events

        private void lvFiles_DoubleClick(object sender, EventArgs e)
        {
            var l = lvFiles.SelectedItems[0];
            if (!(l.Tag is FileInfo file))
                return;

            if (!_validFolderTypes.Contains(file.Type.ToLower()))
                return;

            var path = cbPaths.Text;
            if (!path.EndsWith(_directorySeparator.ToString()))
                path += _directorySeparator;

            path += l.Text;
            cbPaths.Text = path;
            SendGetFilesFolders(path);
            AddAutoCompleteItem(path);
            UpdateButtonStates();
        }

        private void lvFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            var isSelected = lvFiles.SelectedItems.Count == 1;
            var isMultiSelected = lvFiles.SelectedItems.Count >= 1;

            openToolStripMenuItem.Enabled = isMultiSelected;
            //downloadToolStripMenuItem.Enabled = isMultiSelected;
            deleteToolStripMenuItem.Enabled = isMultiSelected;
            renameToolStripMenuItem.Enabled = isSelected;

            if (isSelected)
            {
                //if (!(lvFiles.SelectedItems[0].Tag is FileInfo file))
                //    return;

                //setAsWallpaperToolStripMenuItem.Enabled = IsImage(file.Type);
            }

            long selectedSize = 0;
            foreach (ListViewItem lvi in lvFiles.SelectedItems)
            {
                if (!(lvi.Tag is FileInfo file))
                    continue;

                selectedSize += file.Size;
            }

            lblSelectedItemsCount.Text = lvFiles.SelectedItems.Count > 0
                ? lvFiles.SelectedItems.Count.ToString("N0") + " item" + (isSelected ? "" : "s") + " selected"
                : string.Empty;
            lblSelectedItemsSize.Text = selectedSize > 0
                ? selectedSize.ToBytes() + "  |"
                : string.Empty;
        }

        private void tvPlaces_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvPlaces.SelectedNode.Text.Split(' ')[0] == _machineName)
                return;

            var selected = tvPlaces.SelectedNode;
            var item = selected.Tag as ExplorerPlace;
            var isPersonalFolder = string.Compare("dir", item?.Type, true) == 0;
            var path = isPersonalFolder ? item?.FullPath : item?.Name.Split(' ')[0];
            if (!path.EndsWith(_directorySeparator.ToString()))
                path += _directorySeparator;

            cbPaths.Text = path;
            SendGetFilesFolders(path);

            UpdateButtonStates();
        }

        private void cbSearch_TextChanged(object sender, EventArgs e)
        {
            lvFiles.BeginUpdate();

            lvFiles.Items.Clear();
            var search = cbSearch.Text.ToLower();
            foreach (var file in _fileList)
            {
                if ((!showSystemFilesToolStripMenuItem.Checked && file.IsSystemFile) ||
                    (!showHiddenFilesToolStripMenuItem.Checked && file.IsHiddenFile))
                    continue;

                var fileName = file.FilePath.ToLower();
                if (!(fileName.Contains(search) || fileName.IsLike(search)) && !string.IsNullOrEmpty(search))
                    continue;

                var l = lvFiles.Items.Add(file.FilePath);
                l.Tag = file;
                l.ImageKey = file.Type.TypeToImageKey();
                //l.SubItems.Add(fe.DateModified);
                l.SubItems.Add(file.Type.ExtToType());
                l.SubItems.Add(string.Compare(file.Type, "dir", true) == 0 ? string.Empty : file.Size.ToBytes());

                if (file.IsSystemFile || file.IsHiddenFile)
                {
                    l.ForeColor = Color.Gray;
                }
            }
            lvFiles.EndUpdate();

            cbSearch.AutoCompleteCustomSource.Add(cbSearch.Text);
        }

        private void cbPaths_KeyPress(object sender, KeyPressEventArgs e) => SendGetFilesFolders(cbPaths.Text);

        #endregion

        #region Navigation Buttons

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbPaths.Text))
                return;

            if (btnBack.Enabled)
            {
                int index = _previousPlaces.IndexOf(cbPaths.Text);
                if (index > 0)
                {
                    cbPaths.Text = _previousPlaces[index - 1];
                    SendGetFilesFolders(cbPaths.Text);
                }
            }

            UpdateButtonStates();
        }

        private void BtnForward_Click(object sender, EventArgs e)
        {
            if (btnForward.Enabled)
            {
                int index = _previousPlaces.IndexOf(cbPaths.Text);
                if (index >= 0 && index < _previousPlaces.Count - 1)
                {
                    cbPaths.Text = _previousPlaces[index + 1];
                    SendGetFilesFolders(cbPaths.Text);
                }
            }

            UpdateButtonStates();
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            var path = cbPaths.Text.TrimEnd(_directorySeparator);
            var index = path.LastIndexOf(_directorySeparator);
            // Root of drive/volume
            if (index == -1)
                return;
            path = path.Substring(0, index);
            path += _directorySeparator;

            UpdateButtonStates();

            if (string.IsNullOrEmpty(path))
                return;

            cbPaths.Text = path;

            AddAutoCompleteItem(cbPaths.Text);
            SendGetFilesFolders(cbPaths.Text);
        }

        private void BtnRefresh_Click(object sender, EventArgs e) => SendGetFilesFolders(cbPaths.Text);

        #endregion

        #region Packet Senders

        private void SendGetExplorerPlaces()
        {
            tvPlaces.Nodes.Clear();
            tvPlaces.Nodes.Add(_machineName, _machineName, "computer");

            _client.Send(new DrivePlacesPacket());

            UpdateButtonStates();
        }

        private void SendGetFilesFolders(string path)
        {
            lvFiles.Items.Clear();
            _fileList.Clear();
            _client.Send(new FilePacket(path));
        }

        #endregion

        #region Packet Handlers

        private void HandleDrivePlacesPacket(DrivePlacesPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => HandleDrivePlacesPacket(packet)));
                return;
            }

            _directorySeparator = packet.DirectorySeparator;
            _machineName = packet.MachineName;
            tvPlaces.Nodes[0].Text = packet.MachineName;

            tvPlaces.BeginUpdate();
            foreach (var place in packet.Places)
            {
                // If place type is `dir` then it is a personal folder, otherwise it's a drive
                var name = string.Compare("dir", place.Value, true) == 0
                    ? Path.GetFileName(place.Key)
                    : place.Key;
                var node = tvPlaces.Nodes[0].Nodes.Add(name, name, place.Value.TypeToImageKey(), place.Value.TypeToImageKey());
                node.Tag = new ExplorerPlace
                {
                    FullPath = place.Key,
                    Name = name,
                    Type = place.Value
                };
                tvPlaces.ExpandAll();
            }
            tvPlaces.EndUpdate();
        }

        private void HandleFilesFolders(List<FileInfo> files)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => HandleFilesFolders(files)));
                return;
            }

            lvFiles.Items.Clear();
            lvFiles.BeginUpdate();
            foreach (var file in files)
            {
                _fileList.Add(file);
                if ((file.IsSystemFile && !showSystemFilesToolStripMenuItem.Checked) || (file.IsHiddenFile && !showHiddenFilesToolStripMenuItem.Checked))
                    continue;

                //ListViewItem l = lvFiles.Items.Add(showFileExtensionsToolStripMenuItem.Checked ? file.Path : Path.GetFileNameWithoutExtension(file.Path));
                ListViewItem l = lvFiles.Items.Add(file.FilePath);
                l.Tag = file;
                l.ImageKey = file.Type.TypeToImageKey();
                //l.SubItems.Add(file.DateModified);
                l.SubItems.Add(file.Type.ExtToType());
                l.SubItems.Add(file.Type == "DIR" ? "" : file.Size.ToBytes());

                if (file.IsSystemFile || file.IsHiddenFile)
                {
                    l.ForeColor = Color.Gray;
                }
            }
            lvFiles.EndUpdate();
            lblItemsCount.Text = lvFiles.Items.Count.ToString("N0") + " items  |";
            lblSelectedItemsCount.Text = string.Empty;
            lblSelectedItemsSize.Text = string.Empty;
        }

        #endregion

        #region Type Images

        private void RefreshImageListImages(ImageList imgList)
        {
            imgList.Images.Clear();

            AddImage(imgList, "computer");
            AddImage(imgList, "folder");
            AddImage(imgList, "drive");
            AddImage(imgList, "drive_cd");
            AddImage(imgList, "drive_network");
            AddImage(imgList, "drive_web");
            AddImage(imgList, "page_white");

            AddImage(imgList, "Types\\7z");
            AddImage(imgList, "Types\\asp");
            AddImage(imgList, "Types\\avi");
            AddImage(imgList, "Types\\bin");
            AddImage(imgList, "Types\\bz2");
            AddImage(imgList, "Types\\c");
            AddImage(imgList, "Types\\cat");
            AddImage(imgList, "Types\\cdr");
            AddImage(imgList, "Types\\chm");
            AddImage(imgList, "Types\\class");
            AddImage(imgList, "Types\\cmd");
            AddImage(imgList, "Types\\conf");
            AddImage(imgList, "Types\\cpp");
            AddImage(imgList, "Types\\crt");
            AddImage(imgList, "Types\\cs");
            AddImage(imgList, "Types\\css");
            AddImage(imgList, "Types\\csv");
            AddImage(imgList, "Types\\db");
            AddImage(imgList, "Types\\deb");
            AddImage(imgList, "Types\\dll");
            AddImage(imgList, "Types\\doc");
            AddImage(imgList, "Types\\docx");
            AddImage(imgList, "Types\\dot");
            AddImage(imgList, "Types\\exe");
            AddImage(imgList, "Types\\f4v");
            AddImage(imgList, "Types\\file");
            AddImage(imgList, "Types\\flv");
            AddImage(imgList, "Types\\gif");
            AddImage(imgList, "Types\\gz");
            AddImage(imgList, "Types\\hlp");
            AddImage(imgList, "Types\\htm");
            AddImage(imgList, "Types\\html");
            AddImage(imgList, "Types\\inf");
            AddImage(imgList, "Types\\ini");
            AddImage(imgList, "Types\\iso");
            AddImage(imgList, "Types\\jar");
            AddImage(imgList, "Types\\java");
            AddImage(imgList, "Types\\jpg");
            AddImage(imgList, "Types\\jpeg");
            AddImage(imgList, "Types\\js");
            AddImage(imgList, "Types\\jsp");
            AddImage(imgList, "Types\\lnk");
            AddImage(imgList, "Types\\lua");
            AddImage(imgList, "Types\\m");
            AddImage(imgList, "Types\\mm");
            AddImage(imgList, "Types\\m4a");
            AddImage(imgList, "Types\\m4v");
            AddImage(imgList, "Types\\mov");
            AddImage(imgList, "Types\\mp3");
            AddImage(imgList, "Types\\mp4v");
            AddImage(imgList, "Types\\mpeg");
            AddImage(imgList, "Types\\msi");
            AddImage(imgList, "Types\\pdf");
            AddImage(imgList, "Types\\perl");
            AddImage(imgList, "Types\\pfx");
            AddImage(imgList, "Types\\php");
            AddImage(imgList, "Types\\pl");
            AddImage(imgList, "Types\\png");
            AddImage(imgList, "Types\\ppt");
            AddImage(imgList, "Types\\ps");
            AddImage(imgList, "Types\\psd");
            AddImage(imgList, "Types\\py");
            AddImage(imgList, "Types\\ram");
            AddImage(imgList, "Types\\rar");
            AddImage(imgList, "Types\\reg");
            AddImage(imgList, "Types\\rtf");
            AddImage(imgList, "Types\\ruby");
            AddImage(imgList, "Types\\sig");
            AddImage(imgList, "Types\\sql");
            AddImage(imgList, "Types\\svg");
            AddImage(imgList, "Types\\swf");
            AddImage(imgList, "Types\\sys");
            AddImage(imgList, "Types\\tar");
            AddImage(imgList, "Types\\tgz");
            //AddImage(imgList, "Types\\tif");
            AddImage(imgList, "Types\\ttf");
            AddImage(imgList, "Types\\txt");
            AddImage(imgList, "Types\\url");
            AddImage(imgList, "Types\\vb");
            AddImage(imgList, "Types\\vbs");
            AddImage(imgList, "Types\\vbscript");
            AddImage(imgList, "Types\\vcf");
            AddImage(imgList, "Types\\vdo");
            AddImage(imgList, "Types\\vsd");
            AddImage(imgList, "Types\\wav");
            AddImage(imgList, "Types\\wma");
            AddImage(imgList, "Types\\wmv");
            AddImage(imgList, "Types\\xls");
            AddImage(imgList, "Types\\xml");
            AddImage(imgList, "Types\\zip");
        }

        private void AddImage(ImageList imgList, string path)
        {
            imgList.Images.Add(Path.GetFileName(path), GetImage(path));
        }

        private Image GetImage(string name)
        {
            return Image.FromFile(Path.Combine(Application.StartupPath, $"Resources\\Icons\\Explorer\\{name}.png"));
        }

        #endregion

        #region ContextMenuStrip

        #region View

        private void ViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem item))
                return;

            switch (item.Text)
            {
                case "List":
                    listToolStripMenuItem.Checked = true;
                    detailsToolStripMenuItem.Checked = false;
                    tilesToolStripMenuItem.Checked = false;
                    iconsToolStripMenuItem.Checked = false;

                    imgListTypes.ImageSize = new Size(16, 16);
                    lvFiles.View = View.List;
                    break;
                case "Details":
                    listToolStripMenuItem.Checked = false;
                    detailsToolStripMenuItem.Checked = true;
                    tilesToolStripMenuItem.Checked = false;
                    iconsToolStripMenuItem.Checked = false;

                    imgListTypes.ImageSize = new Size(16, 16);
                    lvFiles.View = View.Details;
                    break;
                case "Tiles":
                    listToolStripMenuItem.Checked = false;
                    detailsToolStripMenuItem.Checked = false;
                    tilesToolStripMenuItem.Checked = true;
                    iconsToolStripMenuItem.Checked = false;

                    imgListTypes.ImageSize = new Size(32, 32);
                    lvFiles.TileSize = new Size(275, 45);
                    lvFiles.View = View.Tile;
                    break;
                case "Icons":
                    listToolStripMenuItem.Checked = false;
                    detailsToolStripMenuItem.Checked = false;
                    tilesToolStripMenuItem.Checked = false;
                    iconsToolStripMenuItem.Checked = true;

                    imgListTypes.ImageSize = new Size(16, 16);
                    lvFiles.View = View.SmallIcon;
                    break;
            }

            RefreshImageListImages(imgListTypes);
        }

        private void ShowFileExtensionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowHideFileExtensions(showFileExtensionsToolStripMenuItem.Checked);
        }

        private void ShowHiddenFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowHideSystemHiddenFiles(showSystemFilesToolStripMenuItem.Checked, showHiddenFilesToolStripMenuItem.Checked);
        }

        private void ShowSystemFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowHideSystemHiddenFiles(showSystemFilesToolStripMenuItem.Checked, showHiddenFilesToolStripMenuItem.Checked);
        }

        #endregion

        private void DownloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count <= 0)
                return;

            foreach (ListViewItem l in lvFiles.SelectedItems)
            {
                if (!(l.Tag is FileInfo file))
                    continue;

                var path = cbPaths.Text + file.FilePath;
                _client.Send(new StartFilePacket(path, SyncIO.Transport.FileTransferType.Download));
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count <= 0)
                return;

            foreach (ListViewItem l in lvFiles.SelectedItems)
            {
                if (!(l.Tag is FileInfo file))
                    continue;

                var path = cbPaths.Text + file.FilePath;
                _client.Send(new OpenFilePacket(path));
            }
        }

        private void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count != 1)
                return;

            if (!(lvFiles.SelectedItems[0].Tag is FileInfo file))
                return;

            var input = InputBoxDialog.GetInput("Enter a new name for the file or directory...", "Enter New Name", file.FilePath);
            if (string.IsNullOrEmpty(input) || string.Compare(file.FilePath, input, true) == 0)
                return;

            _client.Send(new RenameFilePacket
            (
                Path.Combine(cbPaths.Text, file.FilePath),
                Path.Combine(cbPaths.Text, input),
                string.Compare(file.FilePath, "dir", true) == 0)
            );

            file.FilePath = input;
            int index = _fileList.IndexOf(file);
            if (index > -1)
            {
                _fileList.Remove(file);
                _fileList.Insert(index, file);
            }
            lvFiles.SelectedItems[0].Text = input;
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count <= 0)
                return;

            var folders = 0;
            var files = 0;
            var list = new List<ListViewItem>();

            foreach (ListViewItem l in lvFiles.SelectedItems)
            {
                if (!(l.Tag is FileInfo file))
                    continue;

                if (string.Compare(file.Type, "dir", true) == 0)
                    folders++;
                else
                    files++;

                list.Add(l);
            }

            var msg = "{0} folders and {1} files";
            if (folders > 0 && files > 0)
                msg = string.Format("{0} folder(s) and {1} file(s)", folders, files);
            else if (folders > 0 && files == 0)
                msg = string.Format("{0} folder(s)", folders);
            else if (folders == 0 && files > 0)
                msg = string.Format("{0} file(s)", files);

            if (MessageBox.Show(
                string.Format("You are about to delete {0}. Are you sure you want to proceed? This action is irreversible.", msg),
                "Confirmation Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            foreach (ListViewItem l in list)
            {
                if (!(l.Tag is FileInfo file))
                    continue;

                var path = Path.Combine(cbPaths.Text, file.FilePath);
                _client.Send(new DeleteFilePacket(path, file.IsFolder));

                _fileList.Remove(file);
                l.Remove();
            }
        }

        private void NewFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileName = InputBoxDialog.GetInput("Enter a folder name...", "New Folder", "New folder");
            if (string.IsNullOrEmpty(fileName))
                return;

            var path = cbPaths.Text + fileName;
            _client.Send(new NewFilePacket(path, true));
            // TODO: Refresh file list
        }

        #endregion

        #region Helpers

        public void AddAutoCompleteItem(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            cbPaths.AutoCompleteCustomSource.Add(path);
            _previousPlaces.Add(path);
        }

        public void UpdateButtonStates()
        {
            int index = _previousPlaces.IndexOf(cbPaths.Text);
            if (index > 0)
            {
                btnBack.Enabled = cbPaths.Text != _previousPlaces[index - 1];
            }
            else
            {
                btnBack.Enabled = false;
            }

            if (index >= 0 && index < _previousPlaces.Count - 1)
            {
                btnForward.Enabled = cbPaths.Text != _previousPlaces[index + 1];
            }
            else
            {
                btnForward.Enabled = false;
            }

            if (string.IsNullOrEmpty(cbPaths.Text))
            {
                btnUp.Enabled = false;
            }
            else
            {
                btnUp.Enabled = !string.IsNullOrEmpty(Path.GetDirectoryName(cbPaths.Text));
            }
        }

        private void ShowHideFileExtensions(bool show)
        {
            lvFiles.BeginUpdate();
            foreach (ListViewItem l in lvFiles.Items)
            {
                if (!(l.Tag is FileInfo file))
                        continue;

                if (string.Compare(file.Type, "dir", true) == 0)
                    continue;

                l.Text = show ? file.FilePath : Path.GetFileNameWithoutExtension(file.FilePath);
            }
            lvFiles.EndUpdate();
        }

        private void ShowHideSystemHiddenFiles(bool showSystem, bool showHidden)
        {
            lvFiles.BeginUpdate();

            lvFiles.Items.Clear();
            foreach (var file in _fileList)
            {
                if ((!showSystem && file.IsSystemFile) || (!showHidden && file.IsHiddenFile))
                    continue;

                var l = lvFiles.Items.Add(file.FilePath);
                l.Tag = file;
                l.ImageKey = file.Type.TypeToImageKey();
                //l.SubItems.Add(fe.DateModified);
                l.SubItems.Add(file.Type.ExtToType());
                l.SubItems.Add(string.Compare(file.Type, "dir", true) == 0 ? string.Empty : file.Size.ToBytes());

                if (file.IsSystemFile || file.IsHiddenFile)
                {
                    l.ForeColor = Color.Gray;
                }
            }
            lvFiles.EndUpdate();
        }

        #endregion
    }

    class ExplorerPlace
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public string FullPath { get; set; }
    }
}