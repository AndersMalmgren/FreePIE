using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Ffb;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.Strategies;
using PPJoy;
using vJoyInterfaceWrap;

namespace FreePIE.Core.Plugins
{
    [GlobalEnum]
    [Documentation("Directions for vJoy digital POV hats")]
    public enum VJoyPov
    {
        [Documentation("North")]
        Up = 0,
        [Documentation("East")]
        Right = 1,
        [Documentation("South")]
        Down = 2,
        [Documentation("West")]
        Left = 3,
        [Documentation("Centered")]
        Nil = -1
    }

    [GlobalType(Type= typeof(VJoyGlobal), IsIndexed = true)]
    public class VJoyPlugin : Plugin
    {
        private List<VJoyGlobalHolder> holders;

        public override object CreateGlobal()
        {
            holders = new List<VJoyGlobalHolder>();

            return new GlobalIndexer<VJoyGlobal, uint>(Create);
        }

        public override void Stop()
        {
            holders.ForEach(h => h.Dispose());
        }

        private VJoyGlobal Create(uint index)
        {
            var holder = new VJoyGlobalHolder(index + 1);
            holders.Add(holder);

            return holder.Global;
        }

        public override void DoBeforeNextExecute()
        {
            holders.ForEach(h => h.SendPressed());
            holders.ForEach(h => h.PumpFfb());
        }

        public override string FriendlyName
        {
            get { return "vJoy (SourceForge)"; }
        }
    }

    public class VJoyGlobalHolder : IDisposable
    {
        private readonly vJoy joystick;
        private readonly Dictionary<HID_USAGES, bool> enabledAxis;
        private readonly Dictionary<HID_USAGES, int> currentAxisValue; 

        private readonly int maxButtons;
        private readonly int maxDirPov;
        private readonly int maxContinuousPov;
        public const int ContinuousPovMax = 35900;
        
        private readonly SetPressedStrategy setPressedStrategy;
        private FfbSourceGlobal ffb;

        public VJoyGlobalHolder(uint index)
        {
            Index = index;
            Global = new VJoyGlobal(this);
            setPressedStrategy = new SetPressedStrategy(b => SetButton(b, true), b => SetButton(b, false));

            joystick = new vJoy();
            if (index < 1 || index > 16)
                throw new ArgumentException(string.Format("Illegal joystick device id: {0}", index));

            if (!joystick.vJoyEnabled())
                throw new Exception("vJoy driver not enabled: Failed Getting vJoy attributes");

            uint apiVersion = 0;
            uint driverVersion = 0;
            bool match = joystick.DriverMatch(ref apiVersion, ref driverVersion);
            if (!match)
                Console.Error.WriteLine("vJoy version of Driver ({0:X}) does NOT match DLL Version ({1:X})", driverVersion, apiVersion);

            Version = new VjoyVersionGlobal(apiVersion, driverVersion);

            var status = joystick.GetVJDStatus(index);
            
            
            string error = null;
            switch (status)
            {
                case VjdStat.VJD_STAT_BUSY:
                    error = "vJoy Device {0} is already owned by another feeder";
                    break;
                case VjdStat.VJD_STAT_MISS:
                    error = "vJoy Device {0} is not installed or disabled";
                    break;
                case VjdStat.VJD_STAT_UNKN:
                    error = ("vJoy Device {0} general error");
                    break;
            }

            if (error == null && !joystick.AcquireVJD(index))
                error = "Failed to acquire vJoy device number {0}";

            if (error != null)
                throw new Exception(string.Format(error, index));

            long max = 0;
            joystick.GetVJDAxisMax(index, HID_USAGES.HID_USAGE_X, ref max);
            AxisMax = (int)max / 2 - 1;

            enabledAxis = new Dictionary<HID_USAGES, bool>();
            foreach (HID_USAGES axis in Enum.GetValues(typeof (HID_USAGES)))
                enabledAxis[axis] = joystick.GetVJDAxisExist(index, axis);

            maxButtons = joystick.GetVJDButtonNumber(index);
            maxDirPov = joystick.GetVJDDiscPovNumber(index);
            maxContinuousPov = joystick.GetVJDContPovNumber(index);

            currentAxisValue = new Dictionary<HID_USAGES, int>();

            joystick.ResetVJD(index);
        }

        public void SetButton(int button, bool pressed)
        {
            if(button >= maxButtons)
                throw new Exception(string.Format("Maximum buttons are {0}. You need to increase number of buttons in vJoy config", maxButtons));

            joystick.SetBtn(pressed, Index, (uint)button + 1);
        }

        public void SetPressed(int button)
        {
            setPressedStrategy.Add(button);
        }

        public void SetPressed(int button, bool state)
        {
            setPressedStrategy.Add(button, state);
        }

        public void SendPressed()
        {
            setPressedStrategy.Do();
        }

        public void SetAxis(int x, HID_USAGES usage)
        {
            if(!enabledAxis[usage])
                throw new Exception(string.Format("Axis {0} not enabled, enable it from VJoy config", usage));

            joystick.SetAxis(x + AxisMax, Index, usage);
            currentAxisValue[usage] = x;
        }

        public int GetAxis(HID_USAGES usage)
        {
            return currentAxisValue.ContainsKey(usage) ? currentAxisValue[usage] : 0;
        }

        public void SetDirectionalPov(int pov, VJoyPov direction)
        {
            if (pov >=  maxDirPov)
                throw new Exception(string.Format("Maximum digital POV hats are {0}. You need to increase number of digital POV hats in vJoy config", maxDirPov));

            joystick.SetDiscPov((int) direction, Index, (uint)pov+1);
        }

        public void SetContinuousPov(int pov, int value)
        {
            if(pov >= maxContinuousPov)
                throw new Exception(string.Format("Maximum analog POV sticks are {0}. You need to increase number of analog POV hats in vJoy config", maxContinuousPov));

            joystick.SetContPov(value, Index, (uint)pov+1);
        }

        public VJoyGlobal Global { get; private set; }
        public uint Index { get; private set; }
        public int AxisMax { get; private set; }

        public VjoyVersionGlobal Version { get; private set; }

        public FfbSourceGlobal Ffb
        {
            get
            {
                if (ffb != null)
                    return ffb;

                if (!joystick.IsDeviceFfb(Index))
                    throw new Exception(string.Format("Force feedback is not enabled for vJoy device {0}, enable it in vJoyConf", Index));

                var source = new FfbSource();
                VJoyFfbReader.Register(Index, source);

                return ffb = new FfbSourceGlobal(source);
            }
        }

        public void PumpFfb()
        {
            if (ffb != null)
                ffb.Pump();
        }

        public void Dispose()
        {
            if (ffb != null)
                VJoyFfbReader.Unregister(Index);

            joystick.RelinquishVJD(Index);
        }
    }

    [Documentation("The installed vJoy version")]
    public class VjoyVersionGlobal
    {
        [Documentation("Version of the vJoy driver")]
        public uint driver { get; private set; }
        [Documentation("Version of the vJoy interface dll")]
        public uint api { get; private set; }

        public VjoyVersionGlobal(uint driver, uint api)
        {
            this.driver = driver;
            this.api = api;
        }
    }

    [Global(Name = "vJoy")]
    [Documentation("Emulates a vJoy virtual joystick, index by device, vJoy[0] is vJoy device 1")]
    public class VJoyGlobal
    {
        private readonly VJoyGlobalHolder holder;

        public VJoyGlobal(VJoyGlobalHolder holder)
        {
            this.holder = holder;
        }

        [Documentation("Axes go from -axisMax to axisMax")]
        public int axisMax { get { return holder.AxisMax; }}
        [Documentation("Maximum value of an analog POV hat, hundredths of a degree (35900)")]
        public int continuousPovMax { get { return VJoyGlobalHolder.ContinuousPovMax; } }

        [Documentation("X axis, -axisMax to axisMax")]
        public int x
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_X); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_X); }
        }

        [Documentation("Y axis, -axisMax to axisMax")]
        public int y
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_Y); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_Y); }
        }

        [Documentation("Z axis, -axisMax to axisMax")]
        public int z
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_Z); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_Z); }
        }

        [Documentation("X rotation axis, -axisMax to axisMax")]
        public int rx
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_RX); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_RX); }
        }

        [Documentation("Y rotation axis, -axisMax to axisMax")]
        public int ry
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_RY); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_RY); }
        }

        [Documentation("Z rotation axis, -axisMax to axisMax")]
        public int rz
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_RZ); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_RZ); }
        }

        [Documentation("Slider axis, -axisMax to axisMax")]
        public int slider
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_SL0); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_SL0); }
        }

        [Documentation("Dial axis, -axisMax to axisMax")]
        public int dial
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_SL1); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_SL1); }
        }

        [Documentation("Presses or releases the button, buttons are numbered from 0")]
        public void setButton(int button, bool pressed)
        {
            holder.SetButton(button, pressed);
        }

        [Documentation("Clicks the button, it is pressed now and released in the next iteration")]
        public void setPressed(int button)
        {
            holder.SetPressed(button);
        }

        [Documentation("Clicks the button once each time state changes from false to true")]
        public void setPressed(int button, bool state)
        {
            holder.SetPressed(button, state);
        }

        [Documentation("Sets the direction of a digital POV hat, hats are numbered from 0")]
        public void setDigitalPov(int pov, VJoyPov direction)
        {
            holder.SetDirectionalPov(pov, direction);
        }

        [Documentation("Sets an analog POV hat in hundredths of a degree clockwise from north (0 to continuousPovMax), -1 centers it")]
        public void setAnalogPov(int pov, int value)
        {
            holder.SetContinuousPov(pov, value);
        }

        [Documentation("The installed vJoy version")]
        public VjoyVersionGlobal version { get { return holder.Version; } }

        [Documentation("Force feedback sent by games to this device, it must be enabled in vJoyConf")]
        public FfbSourceGlobal ffb { get { return holder.Ffb; } }
    }
}
