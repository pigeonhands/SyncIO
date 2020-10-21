namespace ScreenCaptureClientPlugin
{
    public static class RemoteSettings
    {
        public static int DesktopQuality { get; set; } = 50;

        public static int DesktopFramesPerSecond { get; set; } = 10;

        public static bool UseDesktopMouseInput { get; set; } = false;

        public static bool UseDesktopKeyboardInput { get; set; } = false;
    }
}