using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Strategies;
using SlimDX.DirectInput;

namespace FreePIE.Core.Plugins
{

    // SlimDX key-codes
    [GlobalEnum]
    public enum Key
    {
        D0 = 0,
        D1 = 1,
        D2 = 2,
        D3 = 3,
        D4 = 4,
        D5 = 5,
        D6 = 6,
        D7 = 7,
        D8 = 8,
        D9 = 9,
        A = 10,
        B = 11,
        C = 12,
        D = 13,
        E = 14,
        F = 15,
        G = 16,
        H = 17,
        I = 18,
        J = 19,
        K = 20,
        L = 21,
        M = 22,
        N = 23,
        O = 24,
        P = 25,
        Q = 26,
        R = 27,
        S = 28,
        T = 29,
        U = 30,
        V = 31,
        W = 32,
        X = 33,
        Y = 34,
        Z = 35,
        AbntC1 = 36,
        AbntC2 = 37,
        Apostrophe = 38,
        Applications = 39,
        AT = 40,
        AX = 41,
        Backspace = 42,
        Backslash = 43,
        Calculator = 44,
        CapsLock = 45,
        Colon = 46,
        Comma = 47,
        Convert = 48,
        Delete = 49,
        DownArrow = 50,
        End = 51,
        Equals = 52,
        Escape = 53,
        F1 = 54,
        F2 = 55,
        F3 = 56,
        F4 = 57,
        F5 = 58,
        F6 = 59,
        F7 = 60,
        F8 = 61,
        F9 = 62,
        F10 = 63,
        F11 = 64,
        F12 = 65,
        F13 = 66,
        F14 = 67,
        F15 = 68,
        Grave = 69,
        Home = 70,
        Insert = 71,
        Kana = 72,
        Kanji = 73,
        LeftBracket = 74,
        LeftControl = 75,
        LeftArrow = 76,
        LeftAlt = 77,
        LeftShift = 78,
        LeftWindowsKey = 79,
        Mail = 80,
        MediaSelect = 81,
        MediaStop = 82,
        Minus = 83,
        Mute = 84,
        MyComputer = 85,
        NextTrack = 86,
        NoConvert = 87,
        NumberLock = 88,
        NumberPad0 = 89,
        NumberPad1 = 90,
        NumberPad2 = 91,
        NumberPad3 = 92,
        NumberPad4 = 93,
        NumberPad5 = 94,
        NumberPad6 = 95,
        NumberPad7 = 96,
        NumberPad8 = 97,
        NumberPad9 = 98,
        NumberPadComma = 99,
        NumberPadEnter = 100,
        NumberPadEquals = 101,
        NumberPadMinus = 102,
        NumberPadPeriod = 103,
        NumberPadPlus = 104,
        NumberPadSlash = 105,
        NumberPadStar = 106,
        Oem102 = 107,
        PageDown = 108,
        PageUp = 109,
        Pause = 110,
        Period = 111,
        PlayPause = 112,
        Power = 113,
        PreviousTrack = 114,
        RightBracket = 115,
        RightControl = 116,
        Return = 117,
        RightArrow = 118,
        RightAlt = 119,
        RightShift = 120,
        RightWindowsKey = 121,
        ScrollLock = 122,
        Semicolon = 123,
        Slash = 124,
        Sleep = 125,
        Space = 126,
        Stop = 127,
        PrintScreen = 128,
        Tab = 129,
        Underline = 130,
        Unlabeled = 131,
        UpArrow = 132,
        VolumeDown = 133,
        VolumeUp = 134,
        Wake = 135,
        WebBack = 136,
        WebFavorites = 137,
        WebForward = 138,
        WebHome = 139,
        WebRefresh = 140,
        WebSearch = 141,
        WebStop = 142,
        Yen = 143,
        Unknown = 144,
    }


    [GlobalType(Type = typeof (KeyboardGlobal))]
    public class KeyboardPlugin : Plugin
    {
        // Maps SlimDX key codes to scan codes
        private int[] ScanCodeMap = {
            0x0B, //D0 = 0,
            0x02, //D1 = 1,
            0x03, //D2 = 2,
            0x04, //D3 = 3,
            0x05, //D4 = 4,
            0x06, //D5 = 5,
            0x07, //D6 = 6,
            0x08, //D7 = 7,
            0x09, //D8 = 8,
            0x0A, //D9 = 9,
            0x1E, //A = 10,
            0x30, //B = 11,
            0x2E, //C = 12,
            0x20, //D = 13,
            0x12, //E = 14,
            0x21, //F = 15,
            0x22, //G = 16,
            0x23, //H = 17,
            0x17, //I = 18,
            0x24, //J = 19,
            0x25, //K = 20,
            0x26, //L = 21,
            0x32, //M = 22,
            0x31, //N = 23,
            0x18, //O = 24,
            0x19, //P = 25,
            0x10, //Q = 26,
            0x13, //R = 27,
            0x1F, //S = 28,
            0x14, //T = 29,
            0x16, //U = 30,
            0x2F, //V = 31,
            0x11, //W = 32,
            0x2D, //X = 33,
            0x15, //Y = 34,
            0x2C, //Z = 35,
            0x73, //AbntC1 = 36,
            0x7E, //AbntC2 = 37,
            0x28, //Apostrophe = 38,
            0xDD, //Applications = 39,
            0x91, //AT = 40,
            0x96, //AX = 41,
            0x0E, //Backspace = 42,
            0x2B, //Backslash = 43,
            0xA1, //Calculator = 44,
            0x3A, //CapsLock = 45,
            0x92, //Colon = 46,
            0x33, //Comma = 47,
            0x79, //Convert = 48,
            0xD3, //Delete = 49,
            0xD0, //DownArrow = 50,
            0xCF, //End = 51,
            0x0D, //Equals = 52,
            0x01, //Escape = 53,
            0x3B, //F1 = 54,
            0x3C, //F2 = 55,
            0x3D, //F3 = 56,
            0x3E, //F4 = 57,
            0x3F, //F5 = 58,
            0x40, //F6 = 59,
            0x41, //F7 = 60,
            0x42, //F8 = 61,
            0x43, //F9 = 62,
            0x44, //F10 = 63,
            0x57, //F11 = 64,
            0x58, //F12 = 65,
            0x64, //F13 = 66,
            0x65, //F14 = 67,
            0x66, //F15 = 68,
            0x29, //Grave = 69,
            0xC7, //Home = 70,
            0xD2, //Insert = 71,
            0x70, //Kana = 72,
            0x94, //Kanji = 73,
            0x1A, //LeftBracket = 74,
            0x1D, //LeftControl = 75,
            0xCB, //LeftArrow = 76,
            0x38, //LeftAlt = 77,
            0x2A, //LeftShift = 78,
            0xDB, //LeftWindowsKey = 79,
            0xEC, //Mail = 80,
            0xED, //MediaSelect = 81,
            0xA4, //MediaStop = 82,
            0x0C, //Minus = 83,
            0xA0, //Mute = 84,
            0xEB, //MyComputer = 85,
            0x99, //NextTrack = 86,
            0x7B, //NoConvert = 87,
            0x45, //NumberLock = 88,
            0x52, //NumberPad0 = 89,
            0x4F, //NumberPad1 = 90,
            0x50, //NumberPad2 = 91,
            0x51, //NumberPad3 = 92,
            0x4B, //NumberPad4 = 93,
            0x4C, //NumberPad5 = 94,
            0x4D, //NumberPad6 = 95,
            0x47, //NumberPad7 = 96,
            0x48, //NumberPad8 = 97,
            0x49, //NumberPad9 = 98,
            0xB3, //NumberPadComma = 99,
            0x9C, //NumberPadEnter = 100,
            0x8D, //NumberPadEquals = 101,
            0x4A, //NumberPadMinus = 102,
            0x53, //NumberPadPeriod = 103,
            0x4E, //NumberPadPlus = 104,
            0xB5, //NumberPadSlash = 105,
            0x37, //NumberPadStar = 106,
            0x56, //Oem102 = 107,
            0xD1, //PageDown = 108,
            0xC9, //PageUp = 109,
            0xC5, //Pause/break = 110,              DIK_PAUSE
            0x34, //Period = 111,
            0xA2, //PlayPause = 112,
            0xDE, //Power = 113,
            0x90, //PreviousTrack = 114,
            0x1B, //RightBracket = 115,
            0x9D, //RightControl = 116,
            0x1C, //Return = 117,
            0xCD, //RightArrow = 118,
            0xB8, //RightAlt = 119,
            0x36, //RightShift = 120,
            0xDC, //RightWindowsKey = 121,
            0x46, //ScrollLock = 122,
            0x27, //Semicolon = 123,
            0x35, //Slash = 124,
            0xDF, //Sleep = 125,
            0x39, //Space = 126,
            0x95, //Stop = 127,                     DIK_STOP
            0xB7, //PrintScreen = 128,              DIK_SYSRQ
            0x0F, //Tab = 129,
            0x93, //Underline = 130,
            0x97, //Unlabeled = 131,
            0xC8, //UpArrow = 132,
            0xAE, //VolumeDown = 133,
            0xB0, //VolumeUp = 134,
            0xE3, //Wake = 135,
            0xEA, //WebBack = 136,
            0xE6, //WebFavorites = 137,
            0xE9, //WebForward = 138,
            0xB2, //WebHome = 139,
            0xE7, //WebRefresh = 140,
            0xE5, //WebSearch = 141,
            0xE8, //WebStop = 142,
            0x7D, //Yen = 143,
            0, //Unknown = 144,
        };

        // Maps SlimDX key codes to dwFlag ExtendedKeyMap
        private bool[] ExtendedKeyMap = {
            false, //D0 = 0,
            false, //D1 = 1,
            false, //D2 = 2,
            false, //D3 = 3,
            false, //D4 = 4,
            false, //D5 = 5,
            false, //D6 = 6,
            false, //D7 = 7,
            false, //D8 = 8,
            false, //D9 = 9,
            false, //A = 10,
            false, //B = 11,
            false, //C = 12,
            false, //D = 13,
            false, //E = 14,
            false, //F = 15,
            false, //G = 16,
            false, //H = 17,
            false, //I = 18,
            false, //J = 19,
            false, //K = 20,
            false, //L = 21,
            false, //M = 22,
            false, //N = 23,
            false, //O = 24,
            false, //P = 25,
            false, //Q = 26,
            false, //R = 27,
            false, //S = 28,
            false, //T = 29,
            false, //U = 30,
            false, //V = 31,
            false, //W = 32,
            false, //X = 33,
            false, //Y = 34,
            false, //Z = 35,
            false, //AbntC1 = 36,        not tested
            false, //AbntC2 = 37,        not tested
            false, //Apostrophe = 38,
            true, //Applications = 39,
            false, //AT = 40,
            false, //AX = 41,
            false, //Backspace = 42,
            false, //Backslash = 43,
            true, //Calculator = 44,
            false, //CapsLock = 45,
            true, //Colon = 46,          not tested
            false, //Comma = 47,
            true, //Convert = 48,        not tested
            true, //Delete = 49,
            true, //DownArrow = 50,
            true, //End = 51,
            false, //Equals = 52,
            false, //Escape = 53,
            false, //F1 = 54,
            false, //F2 = 55,
            false, //F3 = 56,
            false, //F4 = 57,
            false, //F5 = 58,
            false, //F6 = 59,
            false, //F7 = 60,
            false, //F8 = 61,
            false, //F9 = 62,
            false, //F10 = 63,
            false, //F11 = 64,
            false, //F12 = 65,
            false, //F13 = 66,
            false, //F14 = 67,
            false, //F15 = 68,
            false, //Grave = 69,
            true, //Home = 70,
            true, //Insert = 71,
            false, //Kana = 72,
            false, //Kanji = 73,
            false, //LeftBracket = 74,
            false, //LeftControl = 75,
            true, //LeftArrow = 76,
            false, //LeftAlt = 77,
            false, //LeftShift = 78,
            true, //LeftWindowsKey = 79,
            true, //Mail = 80,
            true, //MediaSelect = 81,
            true, //MediaStop = 82,
            false, //Minus = 83,
            true, //Mute = 84,
            true, //MyComputer = 85,
            true, //NextTrack = 86,
            false, //NoConvert = 87,     not tested
            false, //NumberLock = 88,
            false, //NumberPad0 = 89,
            false, //NumberPad1 = 90,
            false, //NumberPad2 = 91,
            false, //NumberPad3 = 92,
            false, //NumberPad4 = 93,
            false, //NumberPad5 = 94,
            false, //NumberPad6 = 95,
            false, //NumberPad7 = 96,
            false, //NumberPad8 = 97,
            false, //NumberPad9 = 98,
            false, //NumberPadComma = 99,
            true, //NumberPadEnter = 100,
            false, //NumberPadEquals = 101,
            false, //NumberPadMinus = 102,
            false, //NumberPadPeriod = 103,
            false, //NumberPadPlus = 104,
            true, //NumberPadSlash = 105,
            false, //NumberPadStar = 106,
            false, //Oem102 = 107,
            true, //PageDown = 108,
            true, //PageUp = 109,
            true, //Pause = 110,        buggy
            false, //Period = 111,
            true, //PlayPause = 112,
            true, //Power = 113,         not tested
            true, //PreviousTrack = 114,
            false, //RightBracket = 115,
            true, //RightControl = 116,
            false, //Return = 117,
            true, //RightArrow = 118,
            true, //RightAlt = 119,
            false, //RightShift = 120,
            true, //RightWindowsKey = 121,
            false, //ScrollLock = 122,
            false, //Semicolon = 123,
            false, //Slash = 124,
            true, //Sleep = 125,         not tested
            false, //Space = 126,
            true, //Stop = 127,          not tested
            true, //PrintScreen = 128,
            false, //Tab = 129,
            false, //Underline = 130,    not tested
            false, //Unlabeled = 131,    not tested
            true, //UpArrow = 132,
            true, //VolumeDown = 133,
            true, //VolumeUp = 134,
            true, //Wake = 135,          not tested
            true, //WebBack = 136,
            true, //WebFavorites = 137,
            true, //WebForward = 138,
            true, //WebHome = 139,
            true, //WebRefresh = 140,
            true, //WebSearch = 141,
            true, //WebStop = 142,
            true, //Yen = 143,           not tested
            false, //Unknown = 144,      not tested
        };

        private DirectInput DirectInputInstance = new DirectInput();
        private Keyboard KeyboardDevice;
        private KeyboardState KeyState = new KeyboardState();
        private bool[] MyKeyDown = new bool[150];
        private SetPressedStrategy setKeyPressedStrategy;
        private GetPressedStrategy<int> getKeyPressedStrategy;

        //-----------------------------------------------------------------------
        public override object CreateGlobal()
        {
            return new KeyboardGlobal(this);
        }

        //-----------------------------------------------------------------------
        public override string FriendlyName
        {
            get { return "Keyboard"; }
        }

        //-----------------------------------------------------------------------
        public override System.Action Start()
        {

            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;

            KeyboardDevice = new Keyboard(DirectInputInstance);
            if (KeyboardDevice == null)
                throw new Exception("Failed to create keyboard device");

            KeyboardDevice.SetCooperativeLevel(handle, CooperativeLevel.Background | CooperativeLevel.Nonexclusive);
            KeyboardDevice.Acquire();

            KeyboardDevice.GetCurrentState(ref KeyState);

            setKeyPressedStrategy = new SetPressedStrategy(KeyDown, KeyUp);
            getKeyPressedStrategy = new GetPressedStrategy<int>(IsKeyDown);

            OnStarted(this, new EventArgs());
            return null;
        }

        //-----------------------------------------------------------------------
        public override void Stop()
        {

            // Don't leave any keys pressed
            for (int i = 0; i < MyKeyDown.Length; i++)
            {
                if (MyKeyDown[i])
                    KeyUp(i);
            }

            if (KeyboardDevice != null)
            {
                KeyboardDevice.Unacquire();
                KeyboardDevice.Dispose();
                KeyboardDevice = null;
            }

            if (DirectInputInstance != null)
            {
                DirectInputInstance.Dispose();
                DirectInputInstance = null;
            }
        }

        //-----------------------------------------------------------------------
        public override bool GetProperty(int index, IPluginProperty property)
        {
            return false;
        }

        //-----------------------------------------------------------------------
        public override bool SetProperties(Dictionary<string, object> properties)
        {
            return false;
        }

        //-----------------------------------------------------------------------
        public override void DoBeforeNextExecute()
        {
            // TODO: This polling loop will run at the frequency of the parser.  I should
            // either run a separate thread poller at a higher frequency or use a buffered
            // input model to handle out-of-sync key presses
            KeyboardDevice.GetCurrentState(ref KeyState);

            setKeyPressedStrategy.Do();
        }

        //-----------------------------------------------------------------------
        public bool IsKeyDown(int keycode)
        {
            // Returns true if the key is currently being pressed
            var key = (SlimDX.DirectInput.Key) keycode;
            bool down = KeyState.IsPressed(key) || MyKeyDown[keycode];
            return down;
        }

        //-----------------------------------------------------------------------
        public bool IsKeyUp(int keycode)
        {
            // Returns true if the key is currently being pressed
            var key = (SlimDX.DirectInput.Key) keycode;
            bool up = KeyState.IsReleased(key) && !MyKeyDown[keycode];
            return up;
        }

        //-----------------------------------------------------------------------
        public bool WasKeyPressed(int key)
        {
            return getKeyPressedStrategy.IsPressed(key);
        }

        //--------------------------------------------------------------------------
        private MouseKeyIO.KEYBDINPUT KeyInput(ushort code, uint flag)
        {
            var i = new MouseKeyIO.KEYBDINPUT();
            i.wVk = 0;
            i.wScan = code;
            i.time = 0;
            i.dwExtraInfo = IntPtr.Zero;
            i.dwFlags = flag | MouseKeyIO.KEYEVENTF_SCANCODE;
            return i;
        }

        //--------------------------------------------------------------------------
        public void KeyDown(int code)
        {

            if (!MyKeyDown[code])
            {
                //System.Console.Out.WriteLine("keydown");
                MyKeyDown[code] = true;
                int scancode = ScanCodeMap[code]; // convert the keycode for SendInput

                var input = new MouseKeyIO.INPUT[1];
                input[0].type = MouseKeyIO.INPUT_KEYBOARD;
                if (ExtendedKeyMap[code])
                    input[0].ki = KeyInput((ushort)scancode, MouseKeyIO.KEYEVENTF_EXTENDEDKEY);
                else
                    input[0].ki = KeyInput((ushort)scancode, 0);

                MouseKeyIO.SendInput(1, input, Marshal.SizeOf(input[0].GetType()));

            }
        }

        //--------------------------------------------------------------------------
        public void KeyUp(int code)
        {

            if (MyKeyDown[code])
            {
                //System.Console.Out.WriteLine("keyup");
                MyKeyDown[code] = false;

                int scancode = ScanCodeMap[code]; // convert the keycode for SendInput

                var input = new MouseKeyIO.INPUT[1];
                input[0].type = MouseKeyIO.INPUT_KEYBOARD;
                if (ExtendedKeyMap[code])
                    input[0].ki = KeyInput((ushort)scancode, MouseKeyIO.KEYEVENTF_EXTENDEDKEY | MouseKeyIO.KEYEVENTF_KEYUP);
                else
                    input[0].ki = KeyInput((ushort)scancode, MouseKeyIO.KEYEVENTF_KEYUP);

                MouseKeyIO.SendInput(1, input, Marshal.SizeOf(input[0].GetType()));

            }
        }

        //-----------------------------------------------------------------------
        public void PressAndRelease(int keycode)
        {
            setKeyPressedStrategy.Add(keycode);
        }
    }

    //==========================================================================
    [Global(Name = "keyboard")]
    public class KeyboardGlobal : UpdateblePluginGlobal<KeyboardPlugin>
    {

        //-----------------------------------------------------------------------
        public KeyboardGlobal(KeyboardPlugin plugin) : base(plugin) { }

        //-----------------------------------------------------------------------
        public bool getKeyDown(Key key)
        {
            return plugin.IsKeyDown((int) key);
        }

        //-----------------------------------------------------------------------
        public void setKeyDown(Key key)
        {
            plugin.KeyDown((int) key);
        }

        //-----------------------------------------------------------------------
        public bool getKeyUp(Key key)
        {
            return plugin.IsKeyUp((int) key);
        }

        //-----------------------------------------------------------------------
        public void setKeyUp(Key key)
        {
            plugin.KeyUp((int) key);
        }

        //-----------------------------------------------------------------------
        public void setKey(Key key, bool down)
        {
            if (down)
                plugin.KeyDown((int) key);
            else
                plugin.KeyUp((int) key);
        }

        //-----------------------------------------------------------------------
        public bool getPressed(Key key)
        {
            return plugin.WasKeyPressed((int) key);
        }

        //-----------------------------------------------------------------------
        public void setPressed(Key key)
        {
            plugin.PressAndRelease((int) key);
        }
    }
}
