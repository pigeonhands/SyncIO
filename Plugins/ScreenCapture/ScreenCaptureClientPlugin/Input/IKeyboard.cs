namespace ScreenCaptureClientPlugin.Input
{
    internal interface IKeyboard
    {
        void KeyDown(string key, int modifiers);

        void KeyUp(string key, int modifiers);
    }
}