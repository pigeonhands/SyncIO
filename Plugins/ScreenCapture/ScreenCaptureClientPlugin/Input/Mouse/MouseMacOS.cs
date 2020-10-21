namespace ScreenCaptureClientPlugin.Input.Mouse
{
    using System;
    using System.Runtime.InteropServices;

    class MouseMacOS : IMouse
    {
        public struct CGPoint
        {
            public double X { get; set; }

            public double Y { get; set; }

            public CGPoint(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        // Mouse events
        public enum CGEventType : uint
        {
            kCGEventNull = 0,
            kCGEventLeftMouseDown = 1,
            kCGEventLeftMouseUp = 2,
            kCGEventRightMouseDown = 3,
            kCGEventRightMouseUp = 4,
            kCGEventMouseMoved = 5,
            kCGEventLeftMouseDragged = 6,
            kCGEventRightMouseDragged = 7,
            kCGEventMouseEntered = 8,
            kCGEventMouseExited = 9,
            kCGEventKeyDown = 10,
            kCGEventKeyUp = 11,
            kCGEventFlagsChanged = 12,
            kCGEventScrollWheel = 22,
            kCGEventOtherMouseDown = 25,
            kCGEventOtherMouseUp = 26,
            kCGEventOtherMouseDragged = 27,
            kCGEventTapDisabledByTimeout = 0xFFFFFFFE,
            kCGEventTapDisabledByUserInput = 0xFFFFFFFF
        }

        public enum CGMouseButton
        {
            Left = 0,
            Right, // 1
            Center // 2
        }

        [DllImport("/System/Library/Frameworks/Cocoa.framework/Cocoa")]
        private static extern void CFRelease(IntPtr cfTypeRef);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern IntPtr CGEventCreate(IntPtr cgEventSourceRef);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern CGPoint CGEventGetLocation(IntPtr cgEventRef);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern IntPtr CGEventCreateMouseEvent(IntPtr cgEventSourceRef, CGEventType cgEventType, CGPoint mouseCursorPosition, CGMouseButton cgMouseButton);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern void CGEventPost(uint cgEventTapLocation, IntPtr cgEventRef);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern IntPtr CGEventSourceCreate(int cgEventSourceStateID);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern IntPtr CGMainDisplayID();

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern int CGDisplayPixelsHigh(IntPtr display);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern int CGDisplayPixelsWide(IntPtr display);

        const int kCGHIDEventTap = 0;

        public void LeftClick(double x, double y, int wheelDelta, bool isKeyUp)
        {
            var bounds = GetScreenBounds();
            var mouseLocation = new CGPoint(x * bounds.X, y * bounds.Y);
            if (!isKeyUp)
            {
                // Press down
                Console.WriteLine("Mouse left click down event");
                var clickMouse = CGEventCreateMouseEvent(IntPtr.Zero, CGEventType.kCGEventLeftMouseDown, mouseLocation, CGMouseButton.Left);
                CGEventPost(kCGHIDEventTap, clickMouse);
                CFRelease(clickMouse);
            }
            else
            {
                Console.WriteLine("Mouse left click up event");
                // Release mouse
                var releaseMouse = CGEventCreateMouseEvent(IntPtr.Zero, CGEventType.kCGEventLeftMouseUp, mouseLocation, CGMouseButton.Left);
                CGEventPost(kCGHIDEventTap, releaseMouse);
                CFRelease(releaseMouse);
            }
            System.Threading.Thread.Sleep(10);
        }

        public void RightClick(double x, double y, int wheelData, bool isKeyUp)
        {
            var bounds = GetScreenBounds();
            var mouseLocation = new CGPoint(x * bounds.X, y * bounds.Y);
            if (!isKeyUp)
            {
                // Press down
                Console.WriteLine("Mouse right click down event");
                var clickMouse = CGEventCreateMouseEvent(IntPtr.Zero, CGEventType.kCGEventRightMouseDown, mouseLocation, CGMouseButton.Right);
                CGEventPost(kCGHIDEventTap, clickMouse);
                CFRelease(clickMouse);
            }
            else
            {
                Console.WriteLine("Mouse right click up event");
                // Release mouse
                var releaseMouse = CGEventCreateMouseEvent(IntPtr.Zero, CGEventType.kCGEventRightMouseUp, mouseLocation, CGMouseButton.Right);
                CGEventPost(kCGHIDEventTap, releaseMouse);
                CFRelease(releaseMouse);
            }
            System.Threading.Thread.Sleep(10);
        }


        //[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        //private static extern IntPtr CGWarpMouseCursorPosition(CGPoint newPoint);

        public void Move(double x, double y)
        {
            var bounds = GetScreenBounds();
            var mouseLocation = new CGPoint(x * bounds.X, y * bounds.Y);
            //CGWarpMouseCursorPosition(mouseLocation);
            var move = CGEventCreateMouseEvent(IntPtr.Zero, CGEventType.kCGEventLeftMouseDragged, mouseLocation, CGMouseButton.Left);
            CGEventPost(kCGHIDEventTap, move);
            CFRelease(move);
        }

        private CGPoint GetScreenBounds()
        {
            var mainDisplayId = CGMainDisplayID();
            var width = CGDisplayPixelsWide(mainDisplayId);
            var height = CGDisplayPixelsHigh(mainDisplayId);
            return new CGPoint(width, height);
        }
    }
}