using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
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

            var diDevices = directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);
            var creator = new Func<DeviceInstance, JoystickGlobal>(d =>
            {
                var controller = new Joystick(directInput, d.InstanceGuid);
                controller.SetCooperativeLevel(handle, CooperativeLevel.Exclusive | CooperativeLevel.Background);
                controller.Acquire();

                var device = new Device(controller);
                devices.Add(device);
                return new JoystickGlobal(device);
            });
            
            return new GlobalIndexer<JoystickGlobal, int, string>(intIndex => creator(diDevices[intIndex]),(strIndex, idx) =>
            {
                    var d = diDevices.Where(di => di.InstanceName == strIndex).ToArray();
                    return d.Length > 0 && d.Length > idx ? creator(d[idx]) : null;
            }
            );
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
        private readonly GetPressedStrategy<int> getPressedStrategy;

        public Device(Joystick joystick)
        {
            this.joystick = joystick;
            SetRange(-1000, 1000);
            getPressedStrategy = new GetPressedStrategy<int>(GetDown);
        }

        public void Dispose()
        {
            joystick.Dispose();
        }

        public JoystickState State
        {
            get { return state ?? (state = joystick.GetCurrentState()); }
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
            get { return State.GetPointOfViewControllers(); }
        }
    }
}
