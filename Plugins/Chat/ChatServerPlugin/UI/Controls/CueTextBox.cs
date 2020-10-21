namespace ChatServerPlugin.UI.Controls
{
    using System;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;

    class CueTextBox : TextBox
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern bool SendMessageW
        (
            IntPtr hWnd,
            int msg,
            bool wParam,
            string lParam
        );

        private string _cueText;
        public string CueText
        {
            get { return _cueText; }
            set
            {
                _cueText = value;
                if (Handle != IntPtr.Zero)
                    SendMessageW(Handle, 5377, CueTextFocus, _cueText);
            }
        }

        public bool CueTextFocus { get; set; }
    }
}