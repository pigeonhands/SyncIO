namespace ScreenCaptureClientPlugin.Input.Keyboard
{
    using System;
    using System.Runtime.InteropServices;

    // TODO: Shortcut/combo keys

    class KeyboardMacOS : IKeyboard
    {
        public enum KeyModifiers
        {
            /* modifiers */
            ActiveFlagBit = 0,    /* activate? (activateEvt and mouseDown)*/
            BtnStateBit = 7,    /* state of button?*/
            CmdKeyBit = 8,    /* command key down?*/
            ShiftKeyBit = 9,    /* shift key down?*/
            AlphaLockBit = 10,   /* alpha lock down?*/
            OptionKeyBit = 11,   /* option key down?*/
            ControlKeyBit = 12,   /* control key down?*/
            RightShiftKeyBit = 13,   /* right shift key down? Not supported on Mac OS X.*/
            RightOptionKeyBit = 14,   /* right Option key down? Not supported on Mac OS X.*/
            RightControlKeyBit = 15    /* right Control key down? Not supported on Mac OS X.*/
        }

        public kVK_ANSI KeyToKeyCode(string key)
        {
            switch (key.ToLower())
            {
                case "a":
                    return kVK_ANSI.kVK_ANSI_A;
                case "b":
                    return kVK_ANSI.kVK_ANSI_B;
                case "c":
                    return kVK_ANSI.kVK_ANSI_C;
                case "d":
                    return kVK_ANSI.kVK_ANSI_D;
                case "e":
                    return kVK_ANSI.kVK_ANSI_E;
                case "f":
                    return kVK_ANSI.kVK_ANSI_F;
                case "g":
                    return kVK_ANSI.kVK_ANSI_G;
                case "h":
                    return kVK_ANSI.kVK_ANSI_H;
                case "i":
                    return kVK_ANSI.kVK_ANSI_I;
                case "j":
                    return kVK_ANSI.kVK_ANSI_J;
                case "k":
                    return kVK_ANSI.kVK_ANSI_K;
                case "l":
                    return kVK_ANSI.kVK_ANSI_L;
                case "m":
                    return kVK_ANSI.kVK_ANSI_M;
                case "n":
                    return kVK_ANSI.kVK_ANSI_N;
                case "o":
                    return kVK_ANSI.kVK_ANSI_O;
                case "p":
                    return kVK_ANSI.kVK_ANSI_P;
                case "q":
                    return kVK_ANSI.kVK_ANSI_Z;
                case "r":
                    return kVK_ANSI.kVK_ANSI_R;
                case "s":
                    return kVK_ANSI.kVK_ANSI_S;
                case "t":
                    return kVK_ANSI.kVK_ANSI_T;
                case "u":
                    return kVK_ANSI.kVK_ANSI_U;
                case "v":
                    return kVK_ANSI.kVK_ANSI_V;
                case "w":
                    return kVK_ANSI.kVK_ANSI_W;
                case "x":
                    return kVK_ANSI.kVK_ANSI_X;
                case "y":
                    return kVK_ANSI.kVK_ANSI_Y;
                case "z":
                    return kVK_ANSI.kVK_ANSI_Z;
                case "0":
                case ")":
                    return kVK_ANSI.kVK_ANSI_0;
                case "1":
                case "!":
                    return kVK_ANSI.kVK_ANSI_1;
                case "2":
                case "@":
                    return kVK_ANSI.kVK_ANSI_2;
                case "3":
                case "#":
                    return kVK_ANSI.kVK_ANSI_3;
                case "4":
                case "$":
                    return kVK_ANSI.kVK_ANSI_4;
                case "5":
                case "%":
                    return kVK_ANSI.kVK_ANSI_5;
                case "6":
                case "^":
                    return kVK_ANSI.kVK_ANSI_6;
                case "7":
                case "&":
                    return kVK_ANSI.kVK_ANSI_7;
                case "8":
                case "*":
                    return kVK_ANSI.kVK_ANSI_8;
                case "9":
                case "(":
                    return kVK_ANSI.kVK_ANSI_9;
                case "=":
                case "+":
                    return kVK_ANSI.kVK_ANSI_Equal;
                case "-":
                case "_":
                    return kVK_ANSI.kVK_ANSI_Minus;
                case "]":
                case "}":
                    return kVK_ANSI.kVK_ANSI_RightBracket;
                case "[":
                case "{":
                    return kVK_ANSI.kVK_ANSI_LeftBracket;
                case "\"":
                case "'":
                    return kVK_ANSI.kVK_ANSI_Quote;
                case ";":
                case ":":
                    return kVK_ANSI.kVK_ANSI_Semicolon;
                case "\\":
                case "|":
                    return kVK_ANSI.kVK_ANSI_Backslash;
                case ",":
                case "<":
                    return kVK_ANSI.kVK_ANSI_Comma;
                case "/":
                case "?":
                    return kVK_ANSI.kVK_ANSI_Slash;
                case ".":
                case ">":
                    return kVK_ANSI.kVK_ANSI_Period;
                case "f1":
                    return kVK_ANSI.kVK_F1;
                case "f2":
                    return kVK_ANSI.kVK_F2;
                case "f3":
                    return kVK_ANSI.kVK_F3;
                case "f4":
                    return kVK_ANSI.kVK_F4;
                case "f5":
                    return kVK_ANSI.kVK_F5;
                case "f6":
                    return kVK_ANSI.kVK_F6;
                case "f7":
                    return kVK_ANSI.kVK_F7;
                case "f8":
                    return kVK_ANSI.kVK_F8;
                case "f9":
                    return kVK_ANSI.kVK_F9;
                case "f10":
                    return kVK_ANSI.kVK_F10;
                case "f11":
                    return kVK_ANSI.kVK_F11;
                case "f12":
                    return kVK_ANSI.kVK_F12;
                case "enter":
                    return kVK_ANSI.kVK_ANSI_KeypadEnter;
                case "delete":
                    return kVK_ANSI.kVK_ForwardDelete;
                case "shiftkey":
                    return kVK_ANSI.kVK_Shift;
                case "controlkey":
                    return kVK_ANSI.kVK_Control;
                case "lwin":
                case "rwin":
                case "command":
                    return kVK_ANSI.kVK_Command;
                case "back":
                    return kVK_ANSI.kVK_Delete;
                case "space":
                    return kVK_ANSI.kVK_Space;
                case "left":
                    return kVK_ANSI.kVK_LeftArrow;
                case "right":
                    return kVK_ANSI.kVK_RightArrow;
                case "up":
                    return kVK_ANSI.kVK_UpArrow;
                case "down":
                    return kVK_ANSI.kVK_DownArrow;
                case "option":
                case "menu":
                    return kVK_ANSI.kVK_Option;
                case "capital":
                    return kVK_ANSI.kVK_CapsLock;
                case "tab":
                    return kVK_ANSI.kVK_Tab;
                case "home":
                    return kVK_ANSI.kVK_Home;
                case "end":
                    return kVK_ANSI.kVK_End;
                case "pageup":
                    return kVK_ANSI.kVK_PageUp;
                case "pagedn":
                case "pagedown":
                case "next": // PageDown
                    return kVK_ANSI.kVK_PageDown;
                case "volumedown":
                    return kVK_ANSI.kVK_VolumeDown;
                case "volumeup":
                    return kVK_ANSI.kVK_VolumeUp;
            }
            Console.WriteLine("Key not handled: " + key);
            return 0;
        }

        public enum kVK_ANSI
        {
            kVK_ANSI_A = 0x00,
            kVK_ANSI_S = 0x01,
            kVK_ANSI_D = 0x02,
            kVK_ANSI_F = 0x03,
            kVK_ANSI_H = 0x04,
            kVK_ANSI_G = 0x05,
            kVK_ANSI_Z = 0x06,
            kVK_ANSI_X = 0x07,
            kVK_ANSI_C = 0x08,
            kVK_ANSI_V = 0x09,
            kVK_ANSI_B = 0x0B,
            kVK_ANSI_Q = 0x0C,
            kVK_ANSI_W = 0x0D,
            kVK_ANSI_E = 0x0E,
            kVK_ANSI_R = 0x0F,
            kVK_ANSI_Y = 0x10,
            kVK_ANSI_T = 0x11,
            kVK_ANSI_1 = 0x12,
            kVK_ANSI_2 = 0x13,
            kVK_ANSI_3 = 0x14,
            kVK_ANSI_4 = 0x15,
            kVK_ANSI_6 = 0x16,
            kVK_ANSI_5 = 0x17,
            kVK_ANSI_Equal = 0x18,
            kVK_ANSI_9 = 0x19,
            kVK_ANSI_7 = 0x1A,
            kVK_ANSI_Minus = 0x1B,
            kVK_ANSI_8 = 0x1C,
            kVK_ANSI_0 = 0x1D,
            kVK_ANSI_RightBracket = 0x1E,
            kVK_ANSI_O = 0x1F,
            kVK_ANSI_U = 0x20,
            kVK_ANSI_LeftBracket = 0x21,
            kVK_ANSI_I = 0x22,
            kVK_ANSI_P = 0x23,
            kVK_ANSI_L = 0x25,
            kVK_ANSI_J = 0x26,
            kVK_ANSI_Quote = 0x27,
            kVK_ANSI_K = 0x28,
            kVK_ANSI_Semicolon = 0x29,
            kVK_ANSI_Backslash = 0x2A,
            kVK_ANSI_Comma = 0x2B,
            kVK_ANSI_Slash = 0x2C,
            kVK_ANSI_N = 0x2D,
            kVK_ANSI_M = 0x2E,
            kVK_ANSI_Period = 0x2F,
            kVK_ANSI_Grave = 0x32,
            kVK_ANSI_KeypadDecimal = 0x41,
            kVK_ANSI_KeypadMultiply = 0x43,
            kVK_ANSI_KeypadPlus = 0x45,
            kVK_ANSI_KeypadClear = 0x47,
            kVK_ANSI_KeypadDivide = 0x4B,
            kVK_ANSI_KeypadEnter = 0x4C,
            kVK_ANSI_KeypadMinus = 0x4E,
            kVK_ANSI_KeypadEquals = 0x51,
            kVK_ANSI_Keypad0 = 0x52,
            kVK_ANSI_Keypad1 = 0x53,
            kVK_ANSI_Keypad2 = 0x54,
            kVK_ANSI_Keypad3 = 0x55,
            kVK_ANSI_Keypad4 = 0x56,
            kVK_ANSI_Keypad5 = 0x57,
            kVK_ANSI_Keypad6 = 0x58,
            kVK_ANSI_Keypad7 = 0x59,
            kVK_ANSI_Keypad8 = 0x5B,
            kVK_ANSI_Keypad9 = 0x5C,

            kVK_Return = 0x24,
            kVK_Tab = 0x30,
            kVK_Space = 0x31,
            kVK_Delete = 0x33,
            kVK_Escape = 0x35,
            kVK_Command = 0x37,
            kVK_Shift = 0x38,
            kVK_CapsLock = 0x39,
            kVK_Option = 0x3A,
            kVK_Control = 0x3B,
            kVK_RightShift = 0x3C,
            kVK_RightOption = 0x3D,
            kVK_RightControl = 0x3E,
            kVK_Function = 0x3F,
            kVK_F17 = 0x40,
            kVK_VolumeUp = 0x48,
            kVK_VolumeDown = 0x49,
            kVK_Mute = 0x4A,
            kVK_F18 = 0x4F,
            kVK_F19 = 0x50,
            kVK_F20 = 0x5A,
            kVK_F5 = 0x60,
            kVK_F6 = 0x61,
            kVK_F7 = 0x62,
            kVK_F3 = 0x63,
            kVK_F8 = 0x64,
            kVK_F9 = 0x65,
            kVK_F11 = 0x67,
            kVK_F13 = 0x69,
            kVK_F16 = 0x6A,
            kVK_F14 = 0x6B,
            kVK_F10 = 0x6D,
            kVK_F12 = 0x6F,
            kVK_F15 = 0x71,
            kVK_Help = 0x72,
            kVK_Home = 0x73,
            kVK_PageUp = 0x74,
            kVK_ForwardDelete = 0x75,
            kVK_F4 = 0x76,
            kVK_End = 0x77,
            kVK_F2 = 0x78,
            kVK_PageDown = 0x79,
            kVK_F1 = 0x7A,
            kVK_LeftArrow = 0x7B,
            kVK_RightArrow = 0x7C,
            kVK_DownArrow = 0x7D,
            kVK_UpArrow = 0x7E
        }

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern IntPtr CGEventCreateKeyboardEvent(IntPtr source, kVK_ANSI virtualKey, bool keyDown);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern void CGEventPost(uint cgEventTapLocation, IntPtr cgEventRef);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern void CGEventSetFlags(IntPtr cgEventRef, int cgEventFlags);

        [DllImport("/System/Library/Frameworks/Cocoa.framework/Cocoa")]
        private static extern void CFRelease(IntPtr cfTypeRef);

        const int kCGHIDEventTap = 0;

        public void KeyDown(string key, int modifiers)
        {
            var pressKey = CGEventCreateKeyboardEvent(IntPtr.Zero, KeyToKeyCode(key), true);
            CGEventSetFlags(pressKey, modifiers);
            CGEventPost(kCGHIDEventTap, pressKey);
            CFRelease(pressKey);
        }

        public void KeyUp(string key, int modifiers)
        {
            var releaseKey = CGEventCreateKeyboardEvent(IntPtr.Zero, KeyToKeyCode(key), false);
            if (modifiers > 0)
            {
                CGEventSetFlags(releaseKey, modifiers);
            }
            CGEventPost(kCGHIDEventTap, releaseKey);
            CFRelease(releaseKey);
        }
    }
}