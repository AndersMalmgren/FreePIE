using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class AccelerationGlobal : Subscribable
    {
        private IWiimoteData data;

        public AccelerationGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
        }

        public double x { get { return data.Acceleration.x; } }
        public double y { get { return data.Acceleration.y; } }
        public double z { get { return data.Acceleration.z; } }
    }

    public class NunchuckGlobal : Subscribable
    {
        private IWiimoteData data;

        public NunchuckGlobal(IWiimoteData data, out Action trigger)
            : base(out trigger)
        {
            this.data = data;
            this.buttons = new NunchuckButtonState(data);
        }

        public Acceleration acceleration { get { return data.Nunchuck.Acceleration; } }
        public NunchuckStick stick { get { return data.Nunchuck.Stick; } }
        public NunchuckButtonState buttons { get; private set; }
    }

    public class NunchuckStick
    {
        public NunchuckStick(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double x { get; private set; }
        public double y { get; private set; }
    }
}
