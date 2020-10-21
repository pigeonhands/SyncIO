namespace ScreenCaptureClientPlugin.Input
{
    internal interface IMouse
    {
        void LeftClick(double x, double y, int wheelDelta, bool isKeyUp);

        void RightClick(double x, double y, int wheelDelta, bool isKeyUp);

        void Move(double x, double y);
    }
}