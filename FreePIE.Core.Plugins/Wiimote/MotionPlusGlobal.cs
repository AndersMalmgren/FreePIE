using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class MotionPlusGlobal : Subscribable
    {
        private IWiimoteData data;

        public MotionPlusGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
        }

        public double yaw_down { get { return data.MotionPlus.x; } }
        public double pitch_left { get { return data.MotionPlus.y;} }
        public double roll_left { get { return data.MotionPlus.z; } }
    }
}
