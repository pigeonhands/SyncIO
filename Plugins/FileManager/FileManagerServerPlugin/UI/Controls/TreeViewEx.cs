namespace FileManagerServerPlugin.UI.Controls
{
    using System;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;

    class TreeViewEx : TreeView
    {
        #region Constants

        const int TV_FIRST = 0x1100;
        const int TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;
        const int TVM_GETEXTENDEDSTYLE = TV_FIRST + 45;
        const int TVS_NOHSCROLL = 0x8000;
        const int TVS_EX_AUTOHSCROLL = 0x0020;
        const int TVS_EX_FADEINOUTEXPANDOS = 0x0040;
        const int TVS_EX_DOUBLEBUFFER = 0x0004;

        #endregion

        #region Win32 Imports

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int SendMessage
        (
            IntPtr hWnd,
            uint dwMsg,
            int wParam,
            int lParam
        );

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        static extern int SetWindowTheme
        (
            IntPtr hWnd,
            string pszSubAppName,
            string pszSubIdList
        );

        #endregion

        #region Constructor

        public TreeViewEx()
        {
            HotTracking = true;
        }

        #endregion

        #region Protected Overrides

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= TVS_NOHSCROLL;
                return cp;
            }
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6) // IsVistaOrBetter
            {
                SetWindowTheme(Handle, "explorer", null);

                int style = SendMessage(Handle, Convert.ToUInt32(TVM_GETEXTENDEDSTYLE), 0, 0);
                style |= TVS_EX_AUTOHSCROLL | TVS_EX_FADEINOUTEXPANDOS | TVS_EX_DOUBLEBUFFER;
                SendMessage(Handle, TVM_SETEXTENDEDSTYLE, 0, style);
            }
        }

        #endregion
    }
}