using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.Strategies;
using SlimDX.DirectInput;
using EffectType = vJoyFFBWrapper.EffectType;
using JoystickState = SlimDX.DirectInput.JoystickState;
using vJoyFFBWrapper;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(JoystickGlobal), IsIndexed = true)]
    public class JoystickPlugin : Plugin
    {
        private static List<Device> devices;
        public override object CreateGlobal()
        {
            var directInput = new DirectInput();
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            devices = new List<Device>();

            var diDevices = directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);
            var creator = new Func<DeviceInstance, JoystickGlobal>(d =>
            {
                //prevent a new device from being created (by the second indexer)
                var device = devices.SingleOrDefault(dev => dev.InstanceGuid == d.InstanceGuid);
                if (device != null)
                    return device.global;

                var controller = new Joystick(directInput, d.InstanceGuid);
                controller.SetCooperativeLevel(handle, CooperativeLevel.Exclusive | CooperativeLevel.Background);
                controller.Acquire();

                device = new Device(controller);
                devices.Add(device);
                return new JoystickGlobal(device);
            });

            return new GlobalIndexer<JoystickGlobal, int, string>(index => creator(diDevices[index]), index => creator(diDevices.Single(di => di.InstanceName == index)), () => diDevices.Count);
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

    public partial class Device : IDisposable
    {
        private readonly Joystick joystick;
        private JoystickState state;
        private readonly GetPressedStrategy<int> getPressedStrategy;

        public Device(Joystick joystick)
        {
            this.joystick = joystick;
            SetRange(-1000, 1000);
            getPressedStrategy = new GetPressedStrategy<int>(GetDown);

            if (SupportsFFB)
                PrepareFFB();
        }

        public void Dispose()
        {
            DisposeEffects();
            //Console.WriteLine("Disposing joystick: " + Name);
            joystick.Dispose();
            //Console.WriteLine("Finished disposing joystick");
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

        internal JoystickGlobal global;
    }

    [Global(Name = "joystick")]
    public class JoystickGlobal
    {
        public readonly Device device;

        public JoystickGlobal(Device device)
        {
            this.device = device;
            device.global = this;
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
            get { return State.Z; }
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

        /*	public bool AutoCenter
			{
				get { return device.AutoCenter; }
				set { device.AutoCenter = value; }
			}*/
        public string Name { get { return device.Name; } }
        public bool SupportsFFB { get { return device.SupportsFFB; } }
        public void CreateEffect(int blockIndex, EffectType effectType, int duration, IronPython.Runtime.List dirs)
        {
            device.CreateEffect(blockIndex, effectType, duration, dirs.Cast<int>().ToArray());
        }
        public void SetConstantForce(int blockIndex, int magnitude)
        {
            device.SetConstantForce(blockIndex, magnitude);
        }

        public void OperateEffect(int blockIndex, EffectOperation effectOperation, int loopCount = 0)
        {
            device.OperateEffect(blockIndex, effectOperation, loopCount);
        }
    }
}
