using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using FreePIE.Core.Contracts;
using SlimDX.DirectInput;

namespace FreePIE.Core.Plugins
{

    [LuaGlobalType(Type = typeof(MouseGlobal))]
    public class MousePlugin : Plugin
    {
        // Mouse position state variables
        int DeltaXOut = 0;
        int DeltaYOut = 0;

        DirectInput DirectInputInstance = new DirectInput();
        Mouse MouseDevice;
        MouseState CurrentMouseState;
        bool LeftPressed = false;
        bool RightPressed = false;
        bool MiddlePressed = false;

        //-----------------------------------------------------------------------
        public override object CreateGlobal()
        {
            return new MouseGlobal(this);
        }

        //-----------------------------------------------------------------------
        public override System.Action Start()
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;

            MouseDevice = new Mouse(DirectInputInstance);
            if (MouseDevice == null)
                throw new Exception("Failed to create mouse device");

            MouseDevice.SetCooperativeLevel(handle, CooperativeLevel.Background | CooperativeLevel.Nonexclusive);
            MouseDevice.Properties.AxisMode = DeviceAxisMode.Relative;   // Get delta values
            MouseDevice.Acquire();
          
            OnStarted(this, new EventArgs());
            return null;
        }

        //-----------------------------------------------------------------------
        public override void Stop()
        {
            if (MouseDevice != null)
            {
                MouseDevice.Unacquire();
                MouseDevice.Dispose();
                MouseDevice = null;
            }

            if (DirectInputInstance != null)
            {
                DirectInputInstance.Dispose();
                DirectInputInstance = null;
            }
        }
        
        //---------------------------------------------------------------------
        public override string FriendlyName
        {
            get { return "Mouse"; }
        }

        //---------------------------------------------------------------------
        public override bool GetProperty(int index, IPluginProperty property)
        {
            return false;
        }

        //---------------------------------------------------------------------
        public override bool SetProperties(Dictionary<string, object> properties)
        {
           return true;
        }

        //--------------------------------------------------------------------------
        static private MouseKeyIO.MOUSEINPUT MouseInput(int x, int y, uint data, uint t, uint flag)
        {
           MouseKeyIO.MOUSEINPUT mi = new MouseKeyIO.MOUSEINPUT();
           mi.dx = x;
           mi.dy = y;
           mi.mouseData = data;
           mi.time = t;
           mi.dwFlags = flag;
           return mi;
        }

        //---------------------------------------------------------------------
        public override void DoBeforeNextExecute()
        {
            // If a mouse command was given in the script, issue it all at once right here
            if ((DeltaXOut != 0) || (DeltaYOut != 0))
            {

                MouseKeyIO.INPUT[] input = new MouseKeyIO.INPUT[1];
                input[0].type = MouseKeyIO.INPUT_MOUSE;
                input[0].mi = MouseInput(DeltaXOut, DeltaYOut, 0, 0, MouseKeyIO.MOUSEEVENTF_MOVE);

                MouseKeyIO.SendInput(1, input, Marshal.SizeOf(input[0].GetType()));

                // Reset the mouse values
                DeltaXOut = 0;
                DeltaYOut = 0;
            }

            CurrentMouseState = null;  // flush the mouse state
        }

        //-----------------------------------------------------------------------
        public int DeltaX
        {
            set
            {
                DeltaXOut = value;
            }

            get
            {
                // Retrieve the mouse state only once per iteration to avoid getting
                // zeros on subsequent calls
                if (CurrentMouseState == null)
                    CurrentMouseState = MouseDevice.GetCurrentState();

                return CurrentMouseState.X;
            }
        }

        //-----------------------------------------------------------------------
        public int DeltaY
        {
            set
            {
                DeltaYOut = value;
            }

            get
            {
                // Retrieve the mouse state only once per iteration to avoid getting
                // zeros on subsequent calls
                if (CurrentMouseState == null)
                    CurrentMouseState = MouseDevice.GetCurrentState();

                return CurrentMouseState.Y;
            }
        }

        //-----------------------------------------------------------------------
        public bool IsButtonPressed(int index)
        {

            // Retrieve the mouse state only once per iteration to avoid getting
            // zeros on subsequent calls
            if (CurrentMouseState == null)
                CurrentMouseState = MouseDevice.GetCurrentState();

            return CurrentMouseState.IsPressed(index);
        }

        //----------------------------------------------------------------------
        public void SetButtonPressed(int index, bool pressed)
        {
            uint btn_flag = 0;
            if (index == 0)
            {
               if (pressed)
               {
                  if (!LeftPressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_LEFTDOWN;
               }
               else
               {
                  if (LeftPressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_LEFTUP;
               }
               LeftPressed = pressed;
            }
            else if (index == 1)
            {
               if (pressed)
               {
                  if (!RightPressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_RIGHTDOWN;
               }
               else
               {
                  if (RightPressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_RIGHTUP;
               }
               RightPressed = pressed;
            }
            else
            {
               if (pressed)
               {
                  if (!MiddlePressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_MIDDLEDOWN;
               }
               else
               {
                  if (MiddlePressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_MIDDLEUP;
               }
               MiddlePressed = pressed;
            }
           
            if (btn_flag != 0) {
               MouseKeyIO.INPUT[] input = new MouseKeyIO.INPUT[1];
               input[0].type = MouseKeyIO.INPUT_MOUSE;
               input[0].mi = MouseInput(0, 0, 0, 0, btn_flag);
            
               MouseKeyIO.SendInput(1, input, Marshal.SizeOf(input[0].GetType()));
            }
        }
    }

    //==========================================================================
    [LuaGlobal(Name = "mouse")]
    public class MouseGlobal : UpdateblePluginGlobal
    {

        private readonly MousePlugin Mouse;

        //-----------------------------------------------------------------------
        public MouseGlobal(MousePlugin plugin)
            : base(plugin)
        {
            Mouse = plugin;
        }

        public double DeltaX
        {
            get { return Mouse.DeltaX; }
            set { Mouse.DeltaX = (int) Math.Round(value); }
        }

        public double DeltaY
        {
            get { return Mouse.DeltaY; }
            set { Mouse.DeltaY = (int) Math.Round(value); }
        }

        public bool LeftButton
        {
            get { return Mouse.IsButtonPressed(0); }
            set { Mouse.SetButtonPressed(0, value); }
        }

        public bool MiddleButton
        {
            get { return Mouse.IsButtonPressed(2); }
            set { Mouse.SetButtonPressed(2, value); }
        }

        public bool RightButton
        {
            get { return Mouse.IsButtonPressed(1); }
            set { Mouse.SetButtonPressed(1, value); }
        }
    }
}
