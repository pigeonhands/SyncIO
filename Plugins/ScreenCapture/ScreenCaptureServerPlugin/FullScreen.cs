namespace ScreenCaptureServerPlugin
{
    using System;
    using System.Windows.Forms;

    class FullScreen
    {
        public static bool IsFullScreen = false;

        public static void ToggleFullScreen(Form form)
        {
            if (IsFullScreen)
                LeaveFullScreenMode(form);
            else
                EnterFullScreenMode(form);
        }

        public static void EnterFullScreenMode(Form form)
        {
            if (!IsFullScreen)
            {
                form.WindowState = FormWindowState.Normal;
                form.FormBorderStyle = FormBorderStyle.None;
                form.WindowState = FormWindowState.Maximized;
                IsFullScreen = true;
            }
        }
        public static void LeaveFullScreenMode(Form form)
        {
            if (IsFullScreen)
            {
                form.FormBorderStyle = FormBorderStyle.Sizable;
                form.WindowState = FormWindowState.Normal;
                IsFullScreen = false;
            }
        }
    }
}