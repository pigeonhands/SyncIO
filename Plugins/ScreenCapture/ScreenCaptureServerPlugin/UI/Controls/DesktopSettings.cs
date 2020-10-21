namespace ScreenCaptureServerPlugin.UI.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using PopupControl;

    public partial class DesktopSettings : UserControl
    {
        public bool AutoSaveImages
        {
            get { return chkAutoSave.Checked; }
            set
            {
                chkAutoSave.Checked = value;
                Invalidate();
            }
        }
        public bool InverseImage
        {
            get { return chkInverseImage.Checked; }
            set
            {
                chkInverseImage.Checked = value;
                Invalidate();
            }
        }
        public int ImageQuality
        {
            get { return tbQuality.Value; }
            set
            {
                tbQuality.Value = value;
                Invalidate();
            }
        }
        public int MaxFPS
        {
            get { return tbFramesPerSecond.Value; }
            set
            {
                tbFramesPerSecond.Value = value;
                Invalidate();
            }
        }

        public DesktopSettings()
        {
            InitializeComponent();

            MinimumSize = Size;
            MaximumSize = new Size(Size.Width * 2, Size.Height * 2);
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
        protected override void WndProc(ref Message m)
        {
            if ((Parent as Popup).ProcessResizing(ref m))
            {
                return;
            }
            base.WndProc(ref m);
        }

        private void tbQuality_Scroll(object sender, EventArgs e)
        {
            lblQuality.Text = "Image Quality: " + tbQuality.Value + "%";
        }
        private void tbFramesPerSecond_Scroll(object sender, EventArgs e)
        {
            lblMaxFPS.Text = "Max Frames Per Second: " + tbFramesPerSecond.Value;
        }
    }
}