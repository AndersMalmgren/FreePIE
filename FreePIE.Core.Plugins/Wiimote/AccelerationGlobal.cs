using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class AccelerationGlobal : Calibratable
    {
        private IWiimoteData data;

        public AccelerationGlobal(IWiimoteData data, out Action trigger, out Action calibrated) : base(out trigger, out calibrated)
        {
            this.data = data;
        }

        public double x { get { return data.Acceleration.Value.x; } }
        public double y { get { return data.Acceleration.Value.y; } }
        public double z { get { return data.Acceleration.Value.z; } }
    }

    public class NunchuckGlobal : Subscribable
    {
        private IWiimoteData data;

        public NunchuckGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
            this.buttons = new NunchuckButtonState(data);
        }

        public Acceleration acceleration { get { return data.Nunchuck.Acceleration; } }
        public AnalogStick stick { get { return data.Nunchuck.Stick; } }
        public NunchuckButtonState buttons { get; private set; }
    }

    public class GuitarGlobal : Subscribable
    {
        private IWiimoteData data;

        public GuitarGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
            this.buttons = new GuitarButtonState(data);
        }
        public GuitarButtonState buttons { get; private set; }
        public AnalogStick stick { get { return data.Guitar.Stick; } }
        public TapBar tapbar { get { return data.Guitar.TapBar; } }
        public AnalogTrigger whammy { get { return data.Guitar.Whammy; } }
        public bool IsGH3 { get { return data.Guitar.IsGH3; } }
    }

    public class ClassicControllerGlobal : Subscribable
    {
        private IWiimoteData data;

        public ClassicControllerGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
            this.buttons = new ClassicControllerButtonState(data);
        }
        public ClassicControllerButtonState buttons { get; private set; }
        public AnalogStick leftStick { get { return data.ClassicController.LeftStick; } }
        public AnalogStick rightStick { get { return data.ClassicController.RightStick; } }
        public AnalogTrigger rightTrigger { get { return data.ClassicController.RightTrigger; } }
        public AnalogTrigger leftTrigger { get { return data.ClassicController.LeftTrigger; } }
    }

    public class AnalogStick
    {
        public AnalogStick(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double x { get; private set; }
        public double y { get; private set; }
    }

    public class AnalogTrigger
    {
        public AnalogTrigger(double x)
        {
            this.x = x;
        }

        public double x { get; private set; }
    }
}
