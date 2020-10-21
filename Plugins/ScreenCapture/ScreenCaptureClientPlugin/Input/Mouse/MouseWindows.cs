namespace ScreenCaptureClientPlugin.Input.Mouse
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    internal class MouseWindows : IMouse
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event
        (
            MOUSEEVENTF dwFlags,
            int dx,
            int dy,
            int cButtons,
            int dwExtraInfo
        );

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos
        (
            int x,
            int y
        );

        private enum MOUSEEVENTF
        {
            MOVE = 0x1, //0x0001
            LEFTDOWN = 0x02, //0x0002
            LEFTUP = 0x04, //0x0004
            RIGHTDOWN = 0x08, //0x0008
            RIGHTUP = 0x10, //0x0010
            MIDDLEDOWN = 0x20, //0x0020
            MIDDLEUP = 0x40, //0x0040
            XDOWN = 0x80, //0x0080
            XUP = 0x100,
            WHEEL = 0x800,
            HWHEEL = 0x01000,
            ABSOLUTE = 0x8000
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool EnumDisplaySettingsEx(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }


        private Size GetScreenBounds()
        {
            const int ENUM_CURRENT_SETTINGS = -1;
            DEVMODE devMode = default;
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            EnumDisplaySettingsEx(null, ENUM_CURRENT_SETTINGS, ref devMode, 0);
            return new Size(devMode.dmPelsWidth, devMode.dmPelsHeight);
        }

        public void LeftClick(double x, double y, int wheelDelta, bool isKeyUp)
        {
            var rect = GetScreenBounds();
            x = rect.Width * x;
            y = rect.Height * y;

            SetCursorPos((int)x, (int)y);
            mouse_event(isKeyUp ? MOUSEEVENTF.LEFTUP : MOUSEEVENTF.LEFTDOWN, (int)x, (int)y, wheelDelta * 120, 0);
        }

        public void RightClick(double x, double y, int wheelDelta, bool isKeyUp)
        {
            var rect = GetScreenBounds();
            x = rect.Width * x;
            y = rect.Height * y;

            SetCursorPos((int)x, (int)y);
            mouse_event(isKeyUp ? MOUSEEVENTF.LEFTUP : MOUSEEVENTF.RIGHTDOWN, (int)x, (int)y, wheelDelta * 120, 0);
            //mouse_event(MOUSEEVENTF.RIGHTUP, (int)x, (int)y, wheelDelta * 120, 0);
        }

        public void Move(double x, double y)
        {
            var rect = GetScreenBounds();
            x = rect.Width * x;
            y = rect.Height * y;

            SetCursorPos((int)x, (int)y);
        }
    }
}
// MiddleWheel
// mouse_event(0, 0, 0, wheelDelta * 120, 0);