using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.Strategies;
using FreePIE.Core.Plugins.VJoy;
using SlimDX.DirectInput;
using EffectType = FreePIE.Core.Plugins.VJoy.EffectType;
using JoystickState = SlimDX.DirectInput.JoystickState;
using Device = FreePIE.Core.Plugins.Dx.Device;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(JoystickGlobal), IsIndexed = true)]
    public class JoystickPlugin : Plugin
    {
        private Dictionary<Guid, Device> devices;

        public override object CreateGlobal()
        {
            var directInput = new DirectInput();
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            devices = new Dictionary<Guid, Device>();

            var diDevices = directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);
            var creator = new Func<DeviceInstance, JoystickGlobal>(d =>
            {
                if (devices.ContainsKey(d.InstanceGuid)) return devices[d.InstanceGuid].global;

                var controller = new Joystick(directInput, d.InstanceGuid);
                controller.SetCooperativeLevel(handle, CooperativeLevel.Exclusive | CooperativeLevel.Background);
                controller.Acquire();

                return new JoystickGlobal(devices[d.InstanceGuid] = new Device(controller));
            });

            return new GlobalIndexer<JoystickGlobal, int, string>(index => creator(diDevices[index]), index => creator(diDevices.Single(di => di.InstanceName == index)));
        }

        public override void Stop()
        {
            foreach(var d in devices.Values)
                d.Dispose();
        }

        public override void DoBeforeNextExecute()
        {
            foreach(var d in devices.Values)
                d.Reset();
        }

        public override string FriendlyName
        {
            get { return "Joystick"; }
        }
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

        /*    public bool AutoCenter
            {
                get { return device.AutoCenter; }
                set { device.AutoCenter = value; }
            }*/

        public bool supportsFfb { get { return device.SupportsFfb; } }
        public void createEffect(int blockIndex, EffectType effectType, int duration, int[] dirs)
        {
            device.CreateEffect(blockIndex, effectType, duration, dirs);
        }
        public void setConstantForce(int blockIndex, int magnitude)
        {
            device.SetConstantForce(blockIndex, magnitude);
        }

        public void operateEffect(int blockIndex, EffectOperation effectOperation, int loopCount = 0)
        {
            device.OperateEffect(blockIndex, effectOperation, loopCount);
        }
    }
}
