using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
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

    public class BalanceBoardGlobal : Subscribable
    {
        private IWiimoteData data;

        public BalanceBoardGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
        }
        public BalanceBoardSensorList sensors { get { return data.BalanceBoard.sensors; } }
        public BalanceBoardWeight weight { get { return data.BalanceBoard.weight; } }
        public AnalogStick centerOfGravity { get { return data.BalanceBoard.centerOfGravity; } }
        public override string ToString()
        {
            return data.BalanceBoard.ToString();
        }
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
        public override string ToString()
        {
            return String.Format("x: {0}, y: {0}", x, y);
        }
    }

    public class AnalogTrigger
    {
        public AnalogTrigger(double x)
        {
            this.x = x;
        }

        public double x { get; private set; }
        public override string ToString()
        {
            return String.Format("value: {0}", x);
        }
    }
}
