using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Strategies;
using SlimDX.DirectInput;

namespace FreePIE.Core.Plugins
{
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
        private JoystickState state;
        private readonly GetPressedStrategy getPressedStrategy;

        public Device(Joystick joystick)
        {
            this.joystick = joystick;
            SetRange(-1000, 1000);
            getPressedStrategy = new GetPressedStrategy(GetDown);
        }

        public void Dispose()
        {
            joystick.Dispose();
        }

        public JoystickState State
        {
            get {
                if (state == null)
                    state = joystick.GetCurrentState();

                return state;
            }
        }

        public void Reset()
        {
            state = null;
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
            return getPressedStrategy.IsPressed(button);
        }

        public bool GetDown(int button)
        {
            return State.IsPressed(button);
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

        public void setRange(int lowerRange, int upperRange)
        {
            device.SetRange(lowerRange, upperRange);
        }

        public bool getPressed(int button)
        {
            return device.GetPressed(button);
        }

        public bool getDown(int button)
        {
            return device.GetDown(button);
        }

        public int X
        {
            get { return State.X; }
        }

        public int Y
        {
            get { return State.Y; }
        }

        public int Z
        {
            get { return State.Z;  }
        }

        public int ZRotation
        {
            get { return State.RotationZ; }
        }

        public int[] Sliders
        {
            get { return State.GetSliders(); }
        }

        public int[] Pov
        {
            get { return State.GetPointOfViewControllers(); }
        }
    }
}
