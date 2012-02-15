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
    }

    //==========================================================================
    [LuaGlobal(Name = "mouse")]
    public class MouseGlobal : UpdateblePluginGlobal
    {

        private readonly MousePlugin Mouse;

        //-----------------------------------------------------------------------
        public MouseGlobal(MousePlugin plugin) : base(plugin)
        {
            Mouse = plugin;
        }

        //-----------------------------------------------------------------------
        public void setDeltaX(double x)
        {
            Mouse.DeltaX = (int)Math.Round(x);
        }

        //-----------------------------------------------------------------------
        public void setDeltaY(double y)
        {
            Mouse.DeltaY = (int)Math.Round(y);
        }

        //-----------------------------------------------------------------------
        public double getDeltaX()
        {
            return Mouse.DeltaX;
        }

        //-----------------------------------------------------------------------
        public double getDeltaY()
        {
            return Mouse.DeltaY;
        }

        //-----------------------------------------------------------------------
        public bool getLeftButton()
        {
            return Mouse.IsButtonPressed(0);
        }

        //-----------------------------------------------------------------------
        public bool getMiddleButton()
        {
            return Mouse.IsButtonPressed(2);
        }

        //-----------------------------------------------------------------------
        public bool getRightButton()
        {
            return Mouse.IsButtonPressed(1);
        }
    }
}
