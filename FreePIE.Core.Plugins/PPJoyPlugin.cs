using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Strategies;
using PPJoy;

namespace FreePIE.Core.Plugins
{
    [LuaGlobalEnum]
    public enum AxisTypes
    {
        X           = 0,
        Y           = 1,
        ZAxis       = 2,
        XRotation   = 5,
        YRotation   = 6,
        ZRotation   = 3,
        Slider      = 4,
        Dial        = 7
    }

    [LuaGlobalType(Type = typeof(PPJoyGlobal), IsIndexed = true)]
    public class PPJoyPlugin : Plugin
    {
        private Device[] devices;

        public override object CreateGlobal()
        {
            devices = new DeviceManager().GetAllDevices()
                .Where(d => d.DeviceType == JoystickTypes.Virtual_Joystick)
                .Select((d, index) => new Device(this, new VirtualJoystick(index + 1)))
                .ToArray();
            
            return devices.Select(d => new PPJoyGlobal(d.Joystick.VirtualStickNumber, this))
               .ToArray();
        }

        public override void DoBeforeNextExecute()
        {
            foreach(var device in devices)
            {
                device.SendPressed();

                device.Joystick.SendUpdates();
            }
        }

        public override Action Start()
        {
            CenterAxis();

            return null;
        }

        public override void Stop()
        {
            foreach (var device in devices)
            {
                device.Joystick.Dispose();
            }
        }


        public override string FriendlyName
        {
            get { return "PPJoy"; }
        }

        public void SetAxis(int index, AxisTypes axisType, int value)
        {
            var device = GetDevice(index);

            if(value < device.LowerRange || value > device.UpperRange)
            {
                throw new Exception(string.Format("Valid axis value {0} to {1}", device.LowerRange, device.UpperRange));
            }

            int newValue = (VirtualJoystick.MaxAnalogDataSourceVal/2) + 1;

            if(value < 0)
            {
                var ratio = (double)value/device.LowerRange;
                newValue = newValue - (int)(newValue*ratio);
            } 
            else if(value > 0)
            {
                var ratio = (double)value/device.UpperRange;
                newValue = (int)(newValue*ratio) + newValue;
            }

            device.Joystick.SetAnalogDataSourceValue((int)axisType, newValue);

        }

        public void SetRange(int index, int lowerRange, int upperRange)
        {
            GetDevice(index).SetRange(lowerRange, upperRange);
        }

        public void SetButton(int index, int button, bool pressed)
        {
            GetJoystick(index).SetDigitalDataSourceState(button, pressed);
        }

        public void SetPressed(int index, int button)
        {
            GetDevice(index).AddPressed(button);
        }

        private void CenterAxis()
        {
            foreach (var device in devices)
            {
                foreach (var axis in Enum.GetValues(typeof (AxisTypes)))
                {
                    SetAxis(device.Joystick.VirtualStickNumber, (AxisTypes)axis, 0);
                }
            }
        }

        private Device GetDevice(int index)
        {
            return devices[index - 1];
        }

        private VirtualJoystick GetJoystick(int index)
        {
            return GetDevice(index).Joystick;
        }

        private class Device
        {
            private readonly PPJoyPlugin plugin;
            private SetPressedStrategy setPressedStrategy;
            public int LowerRange { get; private set; }
            public int UpperRange { get; private set; }
            public VirtualJoystick Joystick { get; private set;  }

            public Device(PPJoyPlugin plugin,  VirtualJoystick joystick)
            {
                LowerRange = -1000;
                UpperRange = 1000;

                this.plugin = plugin;
                Joystick = joystick;
                setPressedStrategy = new SetPressedStrategy(OnPress, OnRelease);
            }

            public void SetRange(int lowerRange, int upperRange)
            {
                this.UpperRange = upperRange;
                this.LowerRange = lowerRange;
            }

            public void AddPressed(int button)
            {
                setPressedStrategy.Add(button);
            }

            private void OnPress(int button)
            {
                plugin.SetButton(Joystick.VirtualStickNumber, button, true);
            }

            private void OnRelease(int button)
            {
                plugin.SetButton(Joystick.VirtualStickNumber, button, false);
            }

            public void SendPressed()
            {
                setPressedStrategy.Do();
            }
        }
    }

    [LuaGlobal(Name = "ppJoy")]
    public class PPJoyGlobal
    {
        private readonly int index;
        private readonly PPJoyPlugin plugin;
        
        public PPJoyGlobal(int index, PPJoyPlugin plugin)
        {
            this.index = index;
            this.plugin = plugin;
        }

        public void setAxis(AxisTypes axisType, int value)
        {
            plugin.SetAxis(index, axisType, value);
        }

        public void setRange(int lowerRange, int upperRange)
        {
            plugin.SetRange(index, lowerRange, upperRange);
        }

        public void setPressed(int button)
        {
            plugin.SetPressed(index, button);
        }

        public void setButton(int button, bool pressed)
        {
            plugin.SetButton(index, button, pressed);
        }
    }
}
