using System;
using System.Collections;
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
        private readonly IHandleProvider handleProvider;
        private List<Device> devices;

        public JoystickPlugin(IHandleProvider handleProvider)
        {
            this.handleProvider = handleProvider;
        }

        public override object CreateGlobal()
        {
            var directInput = new DirectInput();
            var handle = handleProvider.Handle;
            devices = new List<Device>();
            var globalCache = new Dictionary<Guid, JoystickGlobal>();

            var diDevices = directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);
            var creator = new Func<DeviceInstance, JoystickGlobal>(d =>
            {
                if (globalCache.ContainsKey(d.InstanceGuid)) return globalCache[d.InstanceGuid];

                var controller = new Joystick(directInput, d.InstanceGuid);
                controller.SetCooperativeLevel(handle, CooperativeLevel.Exclusive | CooperativeLevel.Background);
                controller.Acquire();

                var device = new Device(controller);
                devices.Add(device);
                return globalCache[d.InstanceGuid] = new JoystickGlobal(device);
            });

            return new GlobalIndexer<JoystickGlobal, int, string>(index => creator(diDevices[index]), index => creator(diDevices.Single(di => di.InstanceName == index)));
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
    
    [Global(Name = "joystick")]
    public class JoystickGlobal
    {
        private readonly Device device;

        public JoystickGlobal(Device device)
        {
            this.device = device;
        }

        private JoystickState State { get { return device.State; } }

        internal Device Device {  get { return device; } }

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
        public void createEffect(int blockIndex, EffectType effectType, int duration, IEnumerable dirs)
        {
            device.CreateEffect(blockIndex, effectType, duration, dirs.Cast<int>().ToArray());
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
