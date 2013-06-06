using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class Acceleration : Subscribable
    {
        private IWiimoteData data;

        public Acceleration(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
        }

        public double x { get { return data.Acceleration.Item1; } }
        public double y { get { return data.Acceleration.Item2; } }
        public double z { get { return data.Acceleration.Item3; } }
    }
}
