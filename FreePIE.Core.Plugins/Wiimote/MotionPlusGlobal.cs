using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Wiimote
{
    [Documentation("Angular velocity from the MotionPlus in degrees per second")]
    public class MotionPlusGlobal : Calibratable
    {
        private IWiimoteData data;

        public MotionPlusGlobal(IWiimoteData data, out Action trigger, out Action calibrated) : base(out trigger, out calibrated)
        {
            this.data = data;
        }

        [Documentation("Yaw angular velocity in degrees per second")]
        public double yaw_down { get { return data.MotionPlus.Value.x; } }
        [Documentation("Pitch angular velocity in degrees per second")]
        public double pitch_left { get { return data.MotionPlus.Value.y; } }
        [Documentation("Roll angular velocity in degrees per second")]
        public double roll_left { get { return data.MotionPlus.Value.z; } }
    }
}
