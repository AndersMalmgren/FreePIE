using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Strategies;
using SlimDX.DirectInput;

namespace FreePIE.Core.Plugins
{
        // POV
    [GlobalEnum]
    public enum POV0
    {
        Up = 10000,
        UpRight = 10045,
        Right = 10090,
        DownRight = 10135,
        Down = 10180,
        DownLeft = 10225,
        Left = 10270,
        UpLeft = 10315,
    }

    [GlobalEnum]
    public enum POV1
    {
        Up = 20000,
        UpRight = 20045,
        Right = 20090,
        DownRight = 20135,
        Down = 20180,
        DownLeft = 20225,
        Left = 20270,
        UpLeft = 20315,
    }

    [GlobalEnum]
    public enum POV2
    {
        Up = 30000,
        UpRight2 = 30045,
        Right = 30090,
        DownRight = 30135,
        Down = 30180,
        DownLeft = 30225,
        Left = 30270,
        UpLeft = 30315,
    }
  
    [GlobalEnum]
    public enum POV3
    {
        Up = 40000,
        UpRight = 40045,
        Right = 40090,
        DownRight = 40135,
        Down = 40180,
        DownLeft = 40225,
        Left = 40270,
        UpLeft = 40315,
    }

    [GlobalType(Type = typeof(JoystickGlobal), IsIndexed = true)]
    public class JoystickPlugin : Plugin
    {
        private List<Device> devices;

        public override object CreateGlobal()
        {
            var directInput = new DirectInput();
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            devices = new List<Device>();

            foreach (var device in directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
            {
                var controller = new Joystick(directInput, device.InstanceGuid);
                controller.SetCooperativeLevel(handle, CooperativeLevel.Exclusive | CooperativeLevel.Background);
                controller.Acquire();

                devices.Add(new Device(controller));
            }

            return devices.Select(d => new JoystickGlobal(d)).ToArray();
        }

        public override void Stop()
        {
            devices.ForEach(d => d.Dispose());
        }

        public override void DoBeforeNextExecute()
        {
            devices.ForEach(d => d.Reset());
        }

        public override string FriendlyName
        {
            get { return "Joystick"; }
        }
    }

    public class Device : IDisposable
    {
        private readonly Joystick joystick;
        private JoystickState state,laststate;
        private readonly GetHeldDownStrategy<int> getKeyHeldDownStrategy;
        public Device(Joystick joystick)
        {
            this.joystick = joystick;
            SetRange(-1000, 1000);
            getKeyHeldDownStrategy = new GetHeldDownStrategy<int>(GetDown);
        }

        public void Dispose()
        {
            joystick.Dispose();
        }

        public JoystickState State
        {
            get { return state ; }
        }
        public JoystickState LastState
        {
            get { return laststate; }
        }
        public void Reset()
        {
            laststate = state ?? joystick.GetCurrentState();
            state = joystick.GetCurrentState();
        }

        public void SetRange(int lowerRange, int upperRange)
        {
            foreach (DeviceObjectInstance deviceObject in joystick.GetObjects()) 
            {
                if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                    joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(lowerRange, upperRange);
            }
        }

        public bool GetPressed(int button)
        {
            if (button < 128)
                return LastState.IsReleased(button) && State.IsPressed(button);     //button stick

            int numpov = (button / 10000) - 1;                                      //button pov
            int viewpov = (button % 10000) * 100;
            bool last = LastState.GetPointOfViewControllers()[numpov] == viewpov;
            bool current = State.GetPointOfViewControllers()[numpov] == viewpov;
            return !last && current;          
        }

        public bool GetReleased(int button)
        {
            if (button < 128)
                return LastState.IsPressed(button) && State.IsReleased(button);     //button stick

            int numpov = (button / 10000) - 1;                                      //button pov
            int viewpov = (button % 10000) * 100;
            bool last = LastState.GetPointOfViewControllers()[numpov] == viewpov;
            bool current = State.GetPointOfViewControllers()[numpov] == viewpov;
            return last && !current;          
        }

        public bool GetHeldDown(int button, int lapse)
        {
            if (GetPressed(button))                 // pressed button = start timer
            {
                getKeyHeldDownStrategy.CreateTimerIfNotExist(button, lapse);
                return false;
            }
            if (GetReleased(button))                 // released button = stop timer
            {
                getKeyHeldDownStrategy.StopTimer(button, lapse);
                return false;
            }
            if (GetDown(button)) return getKeyHeldDownStrategy.IsTimeElapsed(button, lapse);

            return false;
        }

        public bool GetPreHeldDown(int button, int lapse)
        {
            return GetHeldDown(button, lapse) && getKeyHeldDownStrategy.IsPressed(button, lapse);
        }

        public bool GetDown(int button)
        {
            if(button < 128)
                return State.IsPressed(button);                  //button stick

            int numpov = (button / 10000) - 1;                   //button pov
            int viewpov = (button % 10000) * 100;
            return State.GetPointOfViewControllers()[numpov] == viewpov;     
        }
    }

    [Global(Name = "joystick")]
    public class JoystickGlobal
    {
        private readonly Device device;

        public JoystickGlobal(Device device)
        {
            this.device = device;
        }

        private JoystickState State { get { return device.State; } }
        private JoystickState LastState { get { return device.LastState; } }

        public void setRange(int lowerRange, int upperRange)
        {
            device.SetRange(lowerRange, upperRange);
        }

        public bool getPressed<T>(T button)
        {
            return device.GetPressed(Convert.ToInt32(button));
        }

        public bool getReleased<T>(T button)
        {
            return device.GetReleased(Convert.ToInt32(button));
        }

        public bool getHeldDown<T>(T button, int lapse)
        {
            return device.GetHeldDown(Convert.ToInt32(button), lapse);
        }

        public bool getPressedHeldDown<T>(T button, int lapse)
        {
            return device.GetPreHeldDown(Convert.ToInt32(button), lapse);
        }

        public bool getDown<T>(T button)
        {
            return device.GetDown(Convert.ToInt32(button));
        }

        public int x
        {
            get { return State.X; }
        }

        public int y
        {
            get { return State.Y; }
        }

        public int z
        {
            get { return State.Z;  }
        }

        public int xRotation
        {
            get { return State.RotationX; }
        }

        public int yRotation
        {
            get { return State.RotationY; }
        }

        public int zRotation
        {
            get { return State.RotationZ; }
        }

        public int[] sliders
        {
            get { return State.GetSliders(); }
        }

        public int[] pov
        {
            get { return State.GetPointOfViewControllers();}
        }
    }
}
