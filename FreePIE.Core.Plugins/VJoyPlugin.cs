using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.Strategies;
using FreePIE.Core.Plugins.VJoy;
using vJoyInterfaceWrap;

namespace FreePIE.Core.Plugins
{
    [GlobalEnum]
    public enum VJoyPov
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        Nil = -1
    }

    [GlobalType(Type = typeof(VJoyGlobal), IsIndexed = true)]
    public class VJoyPlugin : Plugin
    {
        public static List<VJoyGlobalHolder> holders;

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
        }

        public override string FriendlyName
        {
            get { return "vJoy (SourceForge)"; }
        }
    }

    public class VjoyVersionGlobal
    {
        public uint driver { get; private set; }
        public uint api { get; private set; }

        public VjoyVersionGlobal(uint driver, uint api)
        {
            this.driver = driver;
            this.api = api;
        }
    }

    [Global(Name = "vJoy")]
    public class VJoyGlobal
    {
        private readonly VJoyGlobalHolder holder;

        public VJoyGlobal(VJoyGlobalHolder holder)
        {
            this.holder = holder;
        }

        public int axisMax { get { return holder.AxisMax; } }
        public int continuousPovMax { get { return VJoyGlobalHolder.ContinuousPovMax; } }

        public int x
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_X); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_X); }
        }

        public int y
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_Y); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_Y); }
        }

        public int z
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_Z); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_Z); }
        }

        public int rx
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_RX); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_RX); }
        }

        public int ry
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_RY); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_RY); }
        }

        public int rz
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_RZ); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_RZ); }
        }

        public int slider
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_SL0); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_SL0); }
        }

        public int dial
        {
            get { return holder.GetAxis(HID_USAGES.HID_USAGE_SL1); }
            set { holder.SetAxis(value, HID_USAGES.HID_USAGE_SL1); }
        }

        public void setButton(int button, bool pressed)
        {
            holder.SetButton(button, pressed);
        }

        public void setPressed(int button)
        {
            holder.SetPressed(button);
        }

        public void setPressed(int button, bool state)
        {
            holder.SetPressed(button, state);
        }

        public void setDigitalPov(int pov, VJoyPov direction)
        {
            holder.SetDirectionalPov(pov, direction);
        }

        public void setAnalogPov(int pov, int value)
        {
            holder.SetContinuousPov(pov, value);
        }

        public VjoyVersionGlobal version { get { return holder.Version; } }

        public void registerFfbDevice(JoystickGlobal dev)
        {
            holder.RegisterFfbDevice(dev.device);
        }
    }
}
