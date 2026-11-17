using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Wiimote
{
    [Documentation("The Nunchuck extension")]
    public class NunchuckGlobal : Subscribable
    {
        private IWiimoteData data;

        public NunchuckGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
            this.buttons = new NunchuckButtonState(data);
        }

        [Documentation("Acceleration of the Nunchuck in m/s2")]
        public Acceleration acceleration { get { return data.Nunchuck.Acceleration; } }
        [Documentation("The analog stick")]
        public AnalogStick stick { get { return data.Nunchuck.Stick; } }
        [Documentation("The Nunchuck buttons")]
        public NunchuckButtonState buttons { get; private set; }
        public override string ToString()
        {
            return String.Format("Stick: {0}, Buttons: {2}, Acceleration: {1}", stick, acceleration, buttons);
        }
    }

    [Documentation("The Guitar Hero guitar extension")]
    public class GuitarGlobal : Subscribable
    {
        private IWiimoteData data;

        public GuitarGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
            this.buttons = new GuitarButtonState(data);
        }
        [Documentation("The guitar buttons")]
        public GuitarButtonState buttons { get; private set; }
        [Documentation("The analog stick")]
        public AnalogStick stick { get { return data.Guitar.Stick; } }
        [Documentation("The touch bar, not available on Guitar Hero 3 guitars")]
        public TapBar tapbar { get { return data.Guitar.TapBar; } }
        [Documentation("The whammy bar")]
        public AnalogTrigger whammy { get { return data.Guitar.Whammy; } }
        [Documentation("True for a Guitar Hero 3 guitar")]
        public bool isGH3 { get { return data.Guitar.IsGH3; } }
        public override string ToString()
        {
            if (isGH3)
            {
                return String.Format("Stick: {0}, Whammy: {1}, GuitarHeroThree: {2}, Buttons: {3}", stick, whammy, isGH3, buttons);
            }
            return String.Format("Stick: {0}, Whammy: {1}, TapBar: {2}, GuitarHeroThree: {3}, Buttons: {4}", stick, whammy, tapbar, isGH3, buttons);
        }
    }

    [Documentation("The Classic Controller extension")]
    public class ClassicControllerGlobal : Subscribable
    {
        private IWiimoteData data;

        public ClassicControllerGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
            this.buttons = new ClassicControllerButtonState(data);
        }
        [Documentation("The Classic Controller buttons")]
        public ClassicControllerButtonState buttons { get; private set; }
        [Documentation("The left analog stick")]
        public AnalogStick leftStick { get { return data.ClassicController.LeftStick; } }
        [Documentation("The right analog stick")]
        public AnalogStick rightStick { get { return data.ClassicController.RightStick; } }
        [Documentation("The right analog trigger")]
        public AnalogTrigger rightTrigger { get { return data.ClassicController.RightTrigger; } }
        [Documentation("The left analog trigger")]
        public AnalogTrigger leftTrigger { get { return data.ClassicController.LeftTrigger; } }
        public override string ToString()
        {
            return String.Format("LeftStick: {0},  RightStick: {1}, LeftTrigger: {2}, RightTrigger: {3}, Buttons: {4}", leftStick, rightStick, leftTrigger, rightTrigger, buttons);
        }
    }

    [Documentation("The Balance Board")]
    public class BalanceBoardGlobal : Subscribable
    {
        private IWiimoteData data;

        public BalanceBoardGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
        }
        [Documentation("The four weight sensors")]
        public BalanceBoardSensorList sensors { get { return data.BalanceBoard.sensors; } }
        [Documentation("The total weight")]
        public BalanceBoardWeight weight { get { return data.BalanceBoard.weight; } }
        [Documentation("The center of gravity")]
        public AnalogStick centerOfGravity { get { return data.BalanceBoard.centerOfGravity; } }
        public override string ToString()
        {
            return data.BalanceBoard.ToString();
        }
    }

    [Documentation("An analog stick")]
    public class AnalogStick
    {
        public AnalogStick(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        [Documentation("Horizontal position")]
        public double x { get; private set; }
        [Documentation("Vertical position")]
        public double y { get; private set; }
        public override string ToString()
        {
            return String.Format("x: {0}, y: {1}", x, y);
        }
    }

    [Documentation("An analog trigger")]
    public class AnalogTrigger
    {
        public AnalogTrigger(double x)
        {
            this.x = x;
        }

        [Documentation("How far the trigger is pressed")]
        public double x { get; private set; }
        public override string ToString()
        {
            return String.Format("value: {0}", x);
        }
    }
}
