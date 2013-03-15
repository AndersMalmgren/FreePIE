using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Strategies;
using SlimDX.DirectInput;

namespace FreePIE.Core.Plugins
{

    [GlobalType(Type = typeof(MouseGlobal))]
    public class MousePlugin : Plugin
    {
        // Mouse position state variables
        int DeltaXOut;
        int DeltaYOut;

        DirectInput DirectInputInstance = new DirectInput();
        Mouse MouseDevice;
        MouseState CurrentMouseState;
        bool LeftPressed;
        bool RightPressed;
        bool MiddlePressed;
        private GetPressedStrategy getButtonPressedStrategy;
        private SetPressedStrategy setButtonPressedStrategy;

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

            getButtonPressedStrategy = new GetPressedStrategy(IsButtonDown);
            setButtonPressedStrategy = new SetPressedStrategy(SetButtonDown, SetButtonUp);
          
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
           var mi = new MouseKeyIO.MOUSEINPUT {dx = x, dy = y, mouseData = data, time = t, dwFlags = flag};
            return mi;
        }

        //---------------------------------------------------------------------
        public override void DoBeforeNextExecute()
        {
            // If a mouse command was given in the script, issue it all at once right here
            if ((DeltaXOut != 0) || (DeltaYOut != 0))
            {

                var input = new MouseKeyIO.INPUT[1];
                input[0].type = MouseKeyIO.INPUT_MOUSE;
                input[0].mi = MouseInput(DeltaXOut, DeltaYOut, 0, 0, MouseKeyIO.MOUSEEVENTF_MOVE);

                MouseKeyIO.SendInput(1, input, Marshal.SizeOf(input[0].GetType()));

                // Reset the mouse values
                DeltaXOut = 0;
                DeltaYOut = 0;
            }

            CurrentMouseState = null;  // flush the mouse state

            setButtonPressedStrategy.Do();
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
        public bool IsButtonDown(int index)
        {

            // Retrieve the mouse state only once per iteration to avoid getting
            // zeros on subsequent calls
            if (CurrentMouseState == null)
                CurrentMouseState = MouseDevice.GetCurrentState();

            return CurrentMouseState.IsPressed(index);
        }

        public bool IsButtonPressed(int button)
        {
            return getButtonPressedStrategy.IsPressed(button);
        }

        private void SetButtonDown(int button)
        {
            SetButtonPressed(button, true);    
        }

        private void SetButtonUp(int button)
        {
            SetButtonPressed(button, false);
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
               var input = new MouseKeyIO.INPUT[1];
               input[0].type = MouseKeyIO.INPUT_MOUSE;
               input[0].mi = MouseInput(0, 0, 0, 0, btn_flag);
            
               MouseKeyIO.SendInput(1, input, Marshal.SizeOf(input[0].GetType()));
            }
        }

        public void PressAndRelease(int button)
        {
            setButtonPressedStrategy.Add(button);
        }
    }

    //==========================================================================
    [Global(Name = "mouse")]
    public class MouseGlobal : UpdateblePluginGlobal<MousePlugin>
    {
        //-----------------------------------------------------------------------
        public MouseGlobal(MousePlugin plugin) : base(plugin) { }

        public double deltaX
        {
            get { return plugin.DeltaX; }
            set { plugin.DeltaX = (int) Math.Round(value); }
        }

        public double deltaY
        {
            get { return plugin.DeltaY; }
            set { plugin.DeltaY = (int) Math.Round(value); }
        }

        public bool leftButton
        {
            get { return plugin.IsButtonDown(0); }
            set { plugin.SetButtonPressed(0, value); }
        }

        public bool middleButton
        {
            get { return plugin.IsButtonDown(2); }
            set { plugin.SetButtonPressed(2, value); }
        }

        public bool rightButton
        {
            get { return plugin.IsButtonDown(1); }
            set { plugin.SetButtonPressed(1, value); }
        }

        public bool getButton(int button)
        {
            return plugin.IsButtonDown(button);
        }

        public void setButton(int button, bool pressed)
        {
            plugin.SetButtonPressed(button, pressed);
        }

        public bool getPressed(int button)
        {
            return plugin.IsButtonPressed(button);
        }

        public void setPressed(int button)
        {
            plugin.PressAndRelease(button);
        }
    }
}
