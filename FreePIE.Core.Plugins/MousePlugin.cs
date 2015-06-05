using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Strategies;
using SlimDX.DirectInput;
using SlimDX.RawInput;

namespace FreePIE.Core.Plugins
{

    [GlobalType(Type = typeof(MouseGlobal))]
    public class MousePlugin : Plugin
    {
        // Mouse position state variables
        private double deltaXOut;
        private double deltaYOut;
        private int wheel;
        public const int WheelMax = 120;

        private DirectInput directInputInstance = new DirectInput();
        private Mouse mouseDevice;
        private MouseState currentMouseState;
        private bool leftPressed;
        private bool rightPressed;
        private bool middlePressed;
        private GetPressedStrategy<int> getButtonPressedStrategy;
        private GetReleasedStrategy<int> getButtonReleasedStrategy;
        private SetPressedStrategy setButtonPressedStrategy;

        public override object CreateGlobal()
        {
            return new MouseGlobal(this);
        }

        public override Action Start()
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;

            mouseDevice = new Mouse(directInputInstance);
            if (mouseDevice == null)
                throw new Exception("Failed to create mouse device");

            mouseDevice.SetCooperativeLevel(handle, CooperativeLevel.Background | CooperativeLevel.Nonexclusive);
            mouseDevice.Properties.AxisMode = DeviceAxisMode.Relative;   // Get delta values
            mouseDevice.Acquire();

            getButtonReleasedStrategy = new GetReleasedStrategy<int>(IsButtonDown);
            getButtonPressedStrategy = new GetPressedStrategy<int>(IsButtonDown);
            setButtonPressedStrategy = new SetPressedStrategy(SetButtonDown, SetButtonUp);
          
            OnStarted(this, new EventArgs());
            return null;
        }

        public override void Stop()
        {
            if (mouseDevice != null)
            {
                mouseDevice.Unacquire();
                mouseDevice.Dispose();
                mouseDevice = null;
            }

            if (directInputInstance != null)
            {
                directInputInstance.Dispose();
                directInputInstance = null;
            }
        }
        
        public override string FriendlyName
        {
            get { return "Mouse"; }
        }

        static private MouseKeyIO.MOUSEINPUT MouseInput(int x, int y, uint data, uint t, uint flag)
        {
           var mi = new MouseKeyIO.MOUSEINPUT {dx = x, dy = y, mouseData = data, time = t, dwFlags = flag};
            return mi;
        }

        public override void DoBeforeNextExecute()
        {
            // If a mouse command was given in the script, issue it all at once right here
            if ((int)deltaXOut != 0 || (int)deltaYOut != 0 || wheel != 0)
            {

                var input = new MouseKeyIO.INPUT[1];
                input[0].type = MouseKeyIO.INPUT_MOUSE;
                input[0].mi = MouseInput((int)deltaXOut, (int)deltaYOut, (uint)wheel, 0, MouseKeyIO.MOUSEEVENTF_MOVE | MouseKeyIO.MOUSEEVENTF_WHEEL);

                MouseKeyIO.SendInput(1, input, Marshal.SizeOf(input[0].GetType()));

                // Reset the mouse values
                if ((int)deltaXOut != 0)
                {
                    deltaXOut = deltaXOut - (int)deltaXOut;
                }
                if ((int)deltaYOut != 0)
                {
                    deltaYOut = deltaYOut - (int)deltaYOut;
                }

                wheel = 0;
            }

            currentMouseState = null;  // flush the mouse state

            setButtonPressedStrategy.Do();
        }

        public double DeltaX
        {
            set
            {
                deltaXOut = deltaXOut + value;
            }

            get { return CurrentMouseState.X; }
        }

        public double DeltaY
        {
            set
            {
                deltaYOut = deltaYOut + value;
            }

            get { return CurrentMouseState.Y; }
        }

        public int Wheel
        {
            get { return CurrentMouseState.Z; }
            set { wheel = value; }
            
        }

        private MouseState CurrentMouseState
        {
            get
            {
                if (currentMouseState == null)
                    currentMouseState = mouseDevice.GetCurrentState();

                return currentMouseState;
            }
        }

        public bool IsButtonDown(int index)
        {
            return CurrentMouseState.IsPressed(index);
        }

        public bool IsButtonPressed(int button)
        {
            return getButtonPressedStrategy.IsPressed(button);
        }

        public bool IsButtonReleased(int button)
        {
            return getButtonReleasedStrategy.IsReleased(button);
        }

        private void SetButtonDown(int button)
        {
            SetButtonPressed(button, true);    
        }

        private void SetButtonUp(int button)
        {
            SetButtonPressed(button, false);
        }

        public void SetButtonPressed(int index, bool pressed)
        {
            uint btn_flag = 0;
            if (index == 0)
            {
               if (pressed)
               {
                  if (!leftPressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_LEFTDOWN;
               }
               else
               {
                  if (leftPressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_LEFTUP;
               }
               leftPressed = pressed;
            }
            else if (index == 1)
            {
               if (pressed)
               {
                  if (!rightPressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_RIGHTDOWN;
               }
               else
               {
                  if (rightPressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_RIGHTUP;
               }
               rightPressed = pressed;
            }
            else
            {
               if (pressed)
               {
                  if (!middlePressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_MIDDLEDOWN;
               }
               else
               {
                  if (middlePressed)
                     btn_flag = MouseKeyIO.MOUSEEVENTF_MIDDLEUP;
               }
               middlePressed = pressed;
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

    [Global(Name = "mouse")]
    public class MouseGlobal
    {
        private readonly MousePlugin plugin;

        public MouseGlobal(MousePlugin plugin)
        {
            this.plugin = plugin;
        } 

        public int wheelMax
        {
            get { return MousePlugin.WheelMax; }
        }

        public double deltaX
        {
            get { return plugin.DeltaX; }
            set { plugin.DeltaX = value; }
        }

        public double deltaY
        {
            get { return plugin.DeltaY; }
            set { plugin.DeltaY = value; }
        }

        public int wheel
        {
            get { return plugin.Wheel; }
            set { plugin.Wheel = value; }
        }

        public bool wheelUp
        {
            get { return plugin.Wheel == wheelMax; }
            set { plugin.Wheel = value ? wheelMax : 0; }
        }

        public bool wheelDown
        {
            get { return plugin.Wheel == -wheelMax; }
            set { plugin.Wheel = value ? -wheelMax : 0; }
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

        public bool getReleased(int button)
        {
            return plugin.IsButtonReleased(button);
        }

        public void setPressed(int button)
        {
            plugin.PressAndRelease(button);
        }
    }
}
