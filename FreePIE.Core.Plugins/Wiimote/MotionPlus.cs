using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class MotionPlus : Subscribable
    {
        private IWiimoteData data;

        public MotionPlus(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
        }

        public double yaw_down { get { return data.MotionPlus.Item1; } }
        public double pitch_left { get { return data.MotionPlus.Item2;} }
        public double roll_left { get { return data.MotionPlus.Item3; } }
    }
}
