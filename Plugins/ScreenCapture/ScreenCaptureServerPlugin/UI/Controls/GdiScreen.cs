namespace ScreenCaptureServerPlugin.UI.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class GdiScreen : Control
    {
        public Bitmap Screen { get; set; }
        public bool ShowBorders { get; set; }
        public Color BorderColor { get; set; }

        public GdiScreen()
        {
            Screen = new Bitmap(1, 1);
            ShowBorders = true;
            BorderColor = Color.Black;

            Size = new Size(50, 50);
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
        }

        public void Draw(Bitmap img)
        {
            Draw(img, ClientRectangle);
        }
        public void Draw(Image img, Rectangle rect)
        {
            if (Screen.Width * Screen.Height < rect.Width * rect.Height)
            {
                Screen = new Bitmap(rect.Width, rect.Height);
            }

            Graphics.FromImage(Screen).DrawImageUnscaled(img, rect);
            //CreateGraphics().DrawImage(img, new Rectangle(Point.Empty, Size));
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (ShowBorders)
            {
                e.Graphics.DrawRectangle(new Pen(BorderColor), 0, 0, Width - 1, Height - 1);
            }
            e.Graphics.DrawImage(Screen, new Rectangle(Point.Empty, Size));
        }
    }
}