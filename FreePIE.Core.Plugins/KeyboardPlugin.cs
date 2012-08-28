using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Strategies;
using SlimDX.DirectInput;

namespace FreePIE.Core.Plugins
{

    // SlimDX key-codes
    [LuaGlobalEnum]
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


    [LuaGlobalType(Type = typeof (KeyboardGlobal))]
    public class KeyboardPlugin : Plugin
    {
        // Maps SlimDX key codes to virtual key codes
        private int[] KeyCodeMap = {
            0x30, // 0
            0x31,
            0x32,
            0x33,
            0x34,
            0x35,
            0x36,
            0x37,
            0x38,
            0x39, // 9
            0x41, // A
            0x42,
            0x43,
            0x44,
            0x45,
            0x46,
            0x47,
            0x48,
            0x49,
            0x4A,
            0x4B,
            0x4C,
            0x4D,
            0x4E,
            0x4F,
            0x50,
            0x51,
            0x52,
            0x53,
            0x54,
            0x55,
            0x56,
            0x57,
            0x58,
            0x59,
            0x5A, // Z
            0,
            0,
            0,
            0,
            0,
            0,
            0x08, // Backspace
            0xDC, // Backslash
            0,
            0x14,
            0,
            0xBC, // comma
            0,
            0x2E, // delete
            0x2A, // down
            0x23,
            0,
            0x1C, // Escape
            0x70, // F1
            0x71,
            0x72,
            0x73,
            0x74,
            0x75,
            0x76,
            0x77,
            0x78,
            0x79,
            0x7A,
            0x7B,
            0x7C,
            0x7D,
            0x7E, // F15
            0,
            0x24, // Home
            0x2D,
            0,
            0,
            0xDB, // LeftBracket
            0xA2,
            0x26, // left array
            0, // left alt
            0xA1, // left shit
            0x5B,
            0,
            0,
            0,
            0xBD, // minus
            0,
            0,
            0,
            0,
            0x90, // num lock
            0x60, // numpad 0
            0x61,
            0x62,
            0x63,
            0x64,
            0x65,
            0x66,
            0x67,
            0x68,
            0x69, // numpad 9
            0,
            0, // numpad enter
            0,
            0,
            0,
            0,
            0,
            0,
            0xE2,
            0x22, // page down
            0x21, // page up
            0,
            0xBE, // period
            0,
            0,
            0,
            0, // right bracket
            0xA4, // right ctrl
            0x0D, // return
            0x27, // right arrow
            0,
            0xA1, // right shift
            0,
            0x91, // scroll lock
            0,
            0,
            0,
            0x20, // space
            0,
            0,
            0x09, // tab
            0,
            0,
            0x26, // up
            0,
            0,
            0,
            0, // web back
            0,
            0,
            0,
            0,
            0,
            0,
            0, // web stop
            0,
            0
        };

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
            0, //AbntC1 = 36,
            0, //AbntC2 = 37,
            0x28, //Apostrophe = 38,
            0, //Applications = 39,
            0, //AT = 40,
            0, //AX = 41,
            0x0E, //Backspace = 42,
            0x2B, //Backslash = 43,
            0, //Calculator = 44,
            0x3A, //CapsLock = 45,
            0, //Colon = 46,
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
            0, //Grave = 69,
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
            0, //Mail = 80,
            0, //MediaSelect = 81,
            0, //MediaStop = 82,
            0x0C, //Minus = 83,
            0, //Mute = 84,
            0, //MyComputer = 85,
            0, //NextTrack = 86,
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
            0, //Oem102 = 107,
            0xD1, //PageDown = 108,
            0xC9, //PageUp = 109,
            0, //Pause = 110,
            0x34, //Period = 111,
            0, //PlayPause = 112,
            0, //Power = 113,
            0, //PreviousTrack = 114,
            0x1B, //RightBracket = 115,
            0x9D, //RightControl = 116,
            0x1C, //Return = 117,
            0xCD, //RightArrow = 118,
            0xB8, //RightAlt = 119,
            0x36, //RightShift = 120,
            0xDC, //RightWindowsKey = 121,
            0, //ScrollLock = 122,
            0x27, //Semicolon = 123,
            0x35, //Slash = 124,
            0, //Sleep = 125,
            0x39, //Space = 126,
            0x95, //Stop = 127,
            0, //PrintScreen = 128,
            0x0F, //Tab = 129,
            0, //Underline = 130,
            0, //Unlabeled = 131,
            0xC8, //UpArrow = 132,
            0, //VolumeDown = 133,
            0, //VolumeUp = 134,
            0, //Wake = 135,
            0, //WebBack = 136,
            0, //WebFavorites = 137,
            0, //WebForward = 138,
            0, //WebHome = 139,
            0, //WebRefresh = 140,
            0, //WebSearch = 141,
            0, //WebStop = 142,
            0x7D, //Yen = 143,
            0, //Unknown = 144,
        };

        private DirectInput DirectInputInstance = new DirectInput();
        private Keyboard KeyboardDevice;
        private KeyboardState KeyState = new KeyboardState();
        private bool[] MyKeyDown = new bool[150];
        private SetPressedStrategy setKeyPressedStrategy;
        private GetPressedStrategy getKeyPressedStrategy;

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
            getKeyPressedStrategy = new GetPressedStrategy(IsKeyDown);

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
            SlimDX.DirectInput.Key key = (SlimDX.DirectInput.Key) keycode;
            bool down = KeyState.IsPressed(key) || MyKeyDown[keycode];
            return down;
        }

        //-----------------------------------------------------------------------
        public bool IsKeyUp(int keycode)
        {
            // Returns true if the key is currently being pressed
            SlimDX.DirectInput.Key key = (SlimDX.DirectInput.Key) keycode;
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
            MouseKeyIO.KEYBDINPUT i = new MouseKeyIO.KEYBDINPUT();
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
                code = ScanCodeMap[code]; // convert the keycode for SendInput

                MouseKeyIO.INPUT[] input = new MouseKeyIO.INPUT[1];
                input[0].type = MouseKeyIO.INPUT_KEYBOARD;
                input[0].ki = KeyInput((ushort) code, 0);

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

                code = ScanCodeMap[code]; // convert the keycode for SendInput

                MouseKeyIO.INPUT[] input = new MouseKeyIO.INPUT[1];
                input[0].type = MouseKeyIO.INPUT_KEYBOARD;
                input[0].ki = KeyInput((ushort) code, MouseKeyIO.KEYEVENTF_KEYUP);

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
    [LuaGlobal(Name = "keyboard")]
    public class KeyboardGlobal : UpdateblePluginGlobal
    {

        private readonly KeyboardPlugin Keyboard;

        //-----------------------------------------------------------------------
        public KeyboardGlobal(KeyboardPlugin plugin)
            : base(plugin)
        {
            Keyboard = plugin;
        }

        //-----------------------------------------------------------------------
        public bool getKeyDown(Key key)
        {
            return Keyboard.IsKeyDown((int) key);
        }

        //-----------------------------------------------------------------------
        public void setKeyDown(Key key)
        {
            Keyboard.KeyDown((int) key);
        }

        //-----------------------------------------------------------------------
        public bool getKeyUp(Key key)
        {
            return Keyboard.IsKeyUp((int) key);
        }

        //-----------------------------------------------------------------------
        public void setKeyUp(Key key)
        {
            Keyboard.KeyUp((int) key);
        }

        //-----------------------------------------------------------------------
        public void setKey(Key key, bool down)
        {
            if (down)
                Keyboard.KeyDown((int) key);
            else
                Keyboard.KeyUp((int) key);
        }

        //-----------------------------------------------------------------------
        public bool getPressed(Key key)
        {
            return Keyboard.WasKeyPressed((int) key);
        }

        //-----------------------------------------------------------------------
        public void setPressed(Key key)
        {
            Keyboard.PressAndRelease((int) key);
        }
    }
}
