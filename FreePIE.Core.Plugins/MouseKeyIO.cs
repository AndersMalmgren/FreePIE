// The MouseKeyIO class encapsulates the Win32 function SendInput()
// and the supporting data structures that are used to emulate both
// mouse and keyboard input

using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins {

   static class MouseKeyIO {

      // These are copies of DirectX constants
      public const int INPUT_MOUSE = 0;
      public const int INPUT_KEYBOARD = 1;
      public const int INPUT_HARDWARE = 2;
      public const uint KEYEVENTF_EXTENDEDKEY   = 0x0001;
      public const uint KEYEVENTF_KEYUP         = 0x0002;
      public const uint KEYEVENTF_UNICODE       = 0x0004;
      public const uint KEYEVENTF_SCANCODE      = 0x0008;
      public const uint XBUTTON1 = 0x0001;
      public const uint XBUTTON2 = 0x0002;
      public const uint MOUSEEVENTF_MOVE = 0x0001;
      public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
      public const uint MOUSEEVENTF_LEFTUP = 0x0004;
      public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
      public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
      public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
      public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
      public const uint MOUSEEVENTF_XDOWN = 0x0080;
      public const uint MOUSEEVENTF_XUP = 0x0100;
      public const uint MOUSEEVENTF_WHEEL = 0x0800;
      public const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
      public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

      [StructLayout(LayoutKind.Sequential)]
      public struct MOUSEINPUT
      {
         public int dx;
         public int dy;
         public uint mouseData;
         public uint dwFlags;
         public uint time;
         public IntPtr dwExtraInfo;
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct KEYBDINPUT
      {
         public ushort wVk;
         public ushort wScan;
         public uint dwFlags;
         public uint time;
         public IntPtr dwExtraInfo;
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct HARDWAREINPUT
      {
         public uint uMsg;
         public ushort wParamL;
         public ushort wParamH;
      }

      [StructLayout(LayoutKind.Explicit)]
      public struct INPUT
      {
         [FieldOffset(0)]
         public int type;
         [FieldOffset(4)]
         public MOUSEINPUT mi;
         [FieldOffset(4)]
         public KEYBDINPUT ki;
         [FieldOffset(4)]
         public HARDWAREINPUT hi;
      }

      [DllImport("user32.dll", SetLastError = true)]
      public static extern uint SendInput(uint num_inputs, INPUT[] inputs, int size);
   }
}
