using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Ffb;
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
            });
        }

        public override void Stop()
        {
            devices.ForEach(d => d.Dispose());
        }

        public override void DoBeforeNextExecute()
        {
            devices.ForEach(d => d.Reset());
            devices.ForEach(d => d.FlushFfb());
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
        private DirectInputFfbDevice ffbDevice;
        private FfbSinkGlobal ffb;

        public Device(Joystick joystick)
        {
            this.joystick = joystick;
            SetRange(-1000, 1000);
            getPressedStrategy = new GetPressedStrategy<int>(GetDown);
        }

        public void Dispose()
        {
            if (ffbDevice != null)
                ffbDevice.Dispose();

            joystick.Dispose();
        }

        public JoystickState State
        {
            get { return state ?? (state = joystick.GetCurrentState()); }
        }

        public FfbSinkGlobal Ffb
        {
            get
            {
                if (ffb != null)
                    return ffb;

                ffbDevice = new DirectInputFfbDevice(joystick);
                return ffb = new FfbSinkGlobal(ffbDevice);
            }
        }

        public void Reset()
        {
            state = null;
        }

        public void FlushFfb()
        {
            if (ffb != null)
                ffb.Flush();
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
    [Documentation("Reads joysticks and gamepads through DirectInput, index by device number or name, joystick[0] or joystick['name']")]
    public class JoystickGlobal
    {
        private readonly Device device;

        public JoystickGlobal(Device device)
        {
            this.device = device;
        }

        private JoystickState State { get { return device.State; } }

        [Documentation("Force feedback of the device")]
        public FfbSinkGlobal ffb { get { return device.Ffb; } }

        [Documentation("Sets the range of the axes, the default is -1000 to 1000")]
        public void setRange(int lowerRange, int upperRange)
        {
            device.SetRange(lowerRange, upperRange);
        }

        [Documentation("True once when the button goes down, false until it has been released and pressed again")]
        public bool getPressed(int button)
        {
            return device.GetPressed(button);
        }

        [Documentation("True while the button is down")]
        public bool getDown(int button)
        {
            return device.GetDown(button);
        }

        [Documentation("X axis")]
        public int x
        {
            get { return State.X; }
        }

        [Documentation("Y axis")]
        public int y
        {
            get { return State.Y; }
        }

        [Documentation("Z axis")]
        public int z
        {
            get { return State.Z;  }
        }

        [Documentation("X rotation axis")]
        public int xRotation
        {
            get { return State.RotationX; }
        }

        [Documentation("Y rotation axis")]
        public int yRotation
        {
            get { return State.RotationY; }
        }

        [Documentation("Z rotation axis")]
        public int zRotation
        {
            get { return State.RotationZ; }
        }

        [Documentation("The slider axes")]
        public int[] sliders
        {
            get { return State.GetSliders(); }
        }

        [Documentation("The point of view hats, the angle in hundredths of a degree clockwise from north or -1 when centered")]
        public int[] pov
        {
            get { return State.GetPointOfViewControllers(); }
        }
    }
}
