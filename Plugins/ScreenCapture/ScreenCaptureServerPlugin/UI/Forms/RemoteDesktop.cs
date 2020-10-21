namespace ScreenCaptureServerPlugin.UI.Forms
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Forms;

    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.ServerPlugin;

    using PopupControl;
    using ScreenCaptureServerPlugin.UI.Controls;

    public partial class RemoteDesktop : Form
    {
        #region Variables

        private readonly INetHost _netHost;
        private readonly ISyncIOClient _client;
        private readonly FrameRateCounter _desktopFrameCounter;
        private readonly DesktopSettings _settingsPopup;
        private readonly Popup _popup;

        private Bitmap _bg = null;
        private float _currentRatio = 0.0f;
        private long _imgCounter = 0;

        #endregion

        #region Properties

        public bool LockRatio { get; set; }

        #endregion

        #region Constructor

        public RemoteDesktop(INetHost netHost, ISyncIOClient client)
        {
            InitializeComponent();

            _netHost = netHost;
            _client = client;

            _desktopFrameCounter = new FrameRateCounter();

            _popup = new Popup(_settingsPopup = new DesktopSettings())
            {
                Resizable = false,
                AutoClose = false
            };
            _settingsPopup.btnOK.Click += (sender, e) =>
            {
                _popup.Close();
                SendUpdatedDesktopSettings();
            };
            _settingsPopup.btnCancel.Click += (sender, e) => _popup.Close();

            _currentRatio = (float)Height / (float)Width;
            LockRatio = true;
            _netHost.SetHandler<DesktopImagePacket>((c, p) => DrawImage(p.Image));
        }

        #endregion

        #region Form Events

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = true;
            _client.Send(new StopDesktopPacket());
            e.Cancel = false;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            if (!LockRatio)
                _currentRatio = (float)Height / (float)Width;
            else
                Height = (int)(Width * _currentRatio);

            base.OnResizeEnd(e);
        }

        protected override void OnMove(EventArgs e)
        {
            if (_popup.IsHandleCreated)
            {
                _popup.Close();
            }
        }

        #endregion

        #region Menu Items

        private void BtnSingle_Click(object sender, EventArgs e)
        {
            if (btnAutomatic.Checked)
                return;

            btnSingle.Enabled = false;
            _client.Send(new DesktopImagePacket(null));
        }

        private void BtnAutomatic_Click(object sender, EventArgs e)
        {
            if (btnAutomatic.Checked)
                _client.Send(new StartDesktopPacket(_settingsPopup.ImageQuality, _settingsPopup.MaxFPS));
            else
                _client.Send(new StopDesktopPacket());
        }

        private void BtnToggleFullScreen_Click(object sender, EventArgs e) => FullScreen.ToggleFullScreen(this);

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var path = Path.Combine(Globals.DesktopImagesDirectory(_client), $"{DateTime.Now:yyyy-MM-dd}_{_imgCounter}.png");
            gdiScreen1.Screen.Save(path, ImageFormat.Png);
            // TODO: Notify UI of image saved
        }

        #endregion

        #region Draw Image

        private void DrawImage(byte[] data)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => DrawImage(data)));
                return;
            }

            Utils.UpdateScreen(ref _bg, data);

            gdiScreen1.Screen = _bg;
            gdiScreen1.Draw(_bg);

            lblFPS.Text = _desktopFrameCounter.CalculateFrameRate() + " FPS";
            lblSize.Text = data.Length / 1024 + "KB";

            if (!btnSingle.Enabled && !btnAutomatic.Checked)
            {
                btnSingle.Enabled = true;
            }

            if (_settingsPopup.AutoSaveImages)
            {
                var path = Path.Combine(Globals.DesktopImagesDirectory(_client), string.Format("{0}_{1}.png", DateTime.Now.ToString("MM-dd-yyyy"), _imgCounter));
                gdiScreen1.Screen.Save(path, ImageFormat.Png);
                _imgCounter++;
            }
        }

        private void SendUpdatedDesktopSettings()
        {
            _client.Send(new DesktopSettingsPacket(
                _settingsPopup.ImageQuality,
                _settingsPopup.MaxFPS
            ));
        }

        public static Image ByteArrayToImage(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }

        #endregion

        #region Popup Settings Events

        private void PopupSettings_MouseEnter(object sender, EventArgs e) => pbSettings.BackColor = Color.FromArgb(209, 226, 242);

        private void PopupSettings_MouseLeave(object sender, EventArgs e) => pbSettings.BackColor = SystemColors.Control;

        private void PopupSettings_Click(object sender, EventArgs e) => _popup.Show(pbSettings);

        #endregion

        #region Mouse UI Events

        private void gdiScreen1_MouseMove(object sender, MouseEventArgs e)
        {
            if (btnAutomatic.Checked && btnMouse.Checked)
            {
                var x = (double)e.X / (double)gdiScreen1.Width;
                var y = (double)e.Y / (double)gdiScreen1.Height;
                var detents = e.Delta / 120;
                _client.Send(new MouseMovePacket(x, y, detents));
            }
        }

        private void gdiScreen1_MouseUp(object sender, MouseEventArgs e)
        {
            if (btnAutomatic.Checked && btnMouse.Checked)
            {
                var x = (double)e.X / (double)gdiScreen1.Width;
                var y = (double)e.Y / (double)gdiScreen1.Height;
                var detents = e.Delta / 120;
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        _client.Send(new MouseClickPacket(x, y, detents, true, true));
                        break;
                    case MouseButtons.Right:
                        _client.Send(new MouseClickPacket(x, y, detents, true, false));
                        break;
                }
            }
        }

        private void gdiScreen1_MouseDown(object sender, MouseEventArgs e)
        {
            if (btnAutomatic.Checked && btnMouse.Checked)
            {
                var x = (double)e.X / (double)gdiScreen1.Width;
                var y = (double)e.Y / (double)gdiScreen1.Height;
                var detents = e.Delta / 120;
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        _client.Send(new MouseClickPacket(x, y, detents, false, true));
                        break;
                    case MouseButtons.Right:
                        _client.Send(new MouseClickPacket(x, y, detents, false, false));
                        break;
                }
            }
        }

        #endregion

        #region Keyboard UI Events

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (((keyData != Keys.Up) && (keyData != Keys.Down)) && ((keyData != Keys.Left) && (keyData != Keys.Right)))
            {
                return base.ProcessDialogKey(keyData);
            }
            this.OnKeyDown(new KeyEventArgs(keyData));
            return true;
        }

        private void RemoteDesktop_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                if (FullScreen.IsFullScreen)
                {
                    FullScreen.LeaveFullScreenMode(this);
                }
                else
                {
                    FullScreen.EnterFullScreenMode(this);
                }
            }

            if (btnKeyboard.Checked)
            {
                _client.Send(new KeyPacket(e.KeyValue.ToString(), (int)e.Modifiers, true));
            }
        }

        private void RemoteDesktop_KeyUp(object sender, KeyEventArgs e)
        {
            if (btnKeyboard.Checked)
            {
                _client.Send(new KeyPacket(e.KeyValue.ToString(), (int)e.Modifiers, false));
            }
        }

        #endregion
    }
}