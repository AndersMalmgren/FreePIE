using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using FreePIE.Core.Contracts;
using SlimDX.DirectInput;

namespace FreePIE.Core.Plugins {

   // SlimDX key-codes
   [LuaGlobalEnum]
   public enum Key {
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
      
   
   [LuaGlobalType(Type = typeof(KeyboardGlobal))]
   public class KeyboardPlugin : Plugin {

      // Maps SlimDX key codes to Microsoft.DirectX.DirectInput codes ????
// TODO: Figure out a consistent mechanism for key codes (virtual key codes)
// instead of using this hack for key conversion
      int[] KeyCodeMap = {
         11,
         2,
         3,
         4,
         5,
         6,
         7,
         8,
         9,
        10,
        30,
        48,
        46,
        32,
        18,
        33,
        34,
        35,
        23,
        36,
        37,
        38,
        50,
        49,
        24,
        25,
        16,
        19,
        31,
        20,
        22,
        47,
        17,
        45,
        21,
        44
      };

      DirectInput DirectInputInstance = new DirectInput();
      Keyboard KeyboardDevice;
      KeyboardState KeyState = new KeyboardState();
      bool[] Pressed = new bool[150];

      //-----------------------------------------------------------------------
      public override object CreateGlobal() {
         return new KeyboardGlobal(this);
      }

      //-----------------------------------------------------------------------
      public override string FriendlyName { 
         get {
            return "Keyboard";
         }
      }

      //-----------------------------------------------------------------------
      public override System.Action Start() {
       
         IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
                  
         KeyboardDevice = new Keyboard(DirectInputInstance);
         if (KeyboardDevice == null)
            throw new Exception("Failed to create keyboard device");

         KeyboardDevice.SetCooperativeLevel(handle, CooperativeLevel.Background | CooperativeLevel.Nonexclusive);
         KeyboardDevice.Acquire();

         KeyboardDevice.GetCurrentState(ref KeyState);
         
         OnStarted(this, new EventArgs());
         return null;
      }

      //-----------------------------------------------------------------------
      public override void Stop() {

         if (KeyboardDevice != null) {
            KeyboardDevice.Unacquire();
            KeyboardDevice.Dispose();
            KeyboardDevice = null;
         }

         if (DirectInputInstance != null) {
            DirectInputInstance.Dispose();
            DirectInputInstance = null;
         }
      }

      //-----------------------------------------------------------------------
      public override bool GetProperty(int index, IPluginProperty property) {
         return false;
      }

      //-----------------------------------------------------------------------
      public override bool SetProperties(Dictionary<string, object> properties) {
         return false;
      }

      //-----------------------------------------------------------------------
      public override void DoBeforeNextExecute() {
// TODO: This polling loop will run at the frequency of the parser.  I should
// either run a separate thread poller at a higher frequency or use a buffered
// input model to handle out-of-sync key presses
         KeyboardDevice.GetCurrentState(ref KeyState);
      }

      //-----------------------------------------------------------------------
      public bool IsKeyDown(int keycode) {
         // Returns true if the key is currently being pressed
         SlimDX.DirectInput.Key key = (SlimDX.DirectInput.Key)keycode;
         bool down = KeyState.IsPressed(key);
         return down;
      }

      //-----------------------------------------------------------------------
      public bool IsKeyUp(int keycode) {
         // Returns true if the key is currently being pressed
         SlimDX.DirectInput.Key key = (SlimDX.DirectInput.Key)keycode;
         bool up = KeyState.IsReleased(key);
         return up;
      }

      //-----------------------------------------------------------------------
      public bool WasKeyPressed(int key) {
         // Returns true if the key is currently being pressed and this is the
         // first time that this key has been polled during that press
         // Subsequent calls to this function will return false if the key 
         // continue to be pressed.  Once the key has been released the next
         // press will return true
         bool previously_pressed = Pressed[key];
         Pressed[key] = IsKeyDown(key);

         if (previously_pressed) {
            // The key was pressed and returned true on a previous call
            return false;
         }
         else {
            // Return and store the current immediate key state
            Pressed[key] = IsKeyDown(key);
            return Pressed[key];
         }
      }

      //--------------------------------------------------------------------------
      private MouseKeyIO.KEYBDINPUT KeyInput(ushort scan, uint flag) {
         MouseKeyIO.KEYBDINPUT i = new MouseKeyIO.KEYBDINPUT();
         i.wVk = 0;
         i.wScan = scan;
         i.time = 0;
         i.dwExtraInfo = IntPtr.Zero;
         i.dwFlags = flag | MouseKeyIO.KEYEVENTF_SCANCODE;
         return i;
      }
      
      //--------------------------------------------------------------------------
      public void KeyDown(int scancode) {

         scancode = KeyCodeMap[scancode];  // convert the keycode for SendInput
         
         MouseKeyIO.INPUT[] input = new MouseKeyIO.INPUT[1];
         input[0].type = MouseKeyIO.INPUT_KEYBOARD;
         input[0].ki = KeyInput((ushort)scancode, 0);
    
         MouseKeyIO.SendInput(1, input, Marshal.SizeOf(input[0].GetType()));
      }

      //--------------------------------------------------------------------------
       public void KeyUp(int scancode) {

         scancode = KeyCodeMap[scancode];  // convert the keycode for SendInput

         MouseKeyIO.INPUT[] input = new MouseKeyIO.INPUT[1];
         input[0].type = MouseKeyIO.INPUT_KEYBOARD;
         input[0].ki = KeyInput((ushort)scancode, MouseKeyIO.KEYEVENTF_KEYUP);
    
         MouseKeyIO.SendInput(1, input, Marshal.SizeOf(input[0].GetType()));
      }
   }

   //==========================================================================
   [LuaGlobal(Name = "keyboard")]
   public class KeyboardGlobal : UpdateblePluginGlobal {

      private readonly KeyboardPlugin Keyboard;

      //-----------------------------------------------------------------------
      public KeyboardGlobal(KeyboardPlugin plugin) : base(plugin) {
         Keyboard = plugin;
      }

      //-----------------------------------------------------------------------
      public bool getKeyDown(int key) {
         return Keyboard.IsKeyDown(key);
      }

      //-----------------------------------------------------------------------
      public void setKeyDown(int key) {
         Keyboard.KeyDown(key);
      }

      //-----------------------------------------------------------------------
      public bool getKeyUp(int key) {
         return Keyboard.IsKeyUp(key);
      }

      //-----------------------------------------------------------------------
      public void setKeyUp(int key) {
         Keyboard.KeyUp(key);
      }

      //-----------------------------------------------------------------------
      public bool getPressed(int key) {
         return Keyboard.WasKeyPressed(key);
      }
   }

}
