using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class WiimoteCalibration
    {
        public WiimoteCalibration(double accGain, double accOffset, double motionplusGainSlow, double motionPlusGainFast, double motionplusOffset)
        {
            this.AccelerationGain = accGain;
            this.AccelerationOffset = accOffset;
            this.MotionPlusGainSlow = motionplusGainSlow;
            this.MotionPlusGainFast = motionPlusGainFast;
            this.MotionPlusOffset = motionplusOffset;
        }

        public double AccelerationGain { get; set; }

        public double AccelerationOffset { get; set; }

        public double MotionPlusGainSlow { get; set; }

        public double MotionPlusGainFast { get; set; }

        public double MotionPlusOffset { get; set; }
    }
}
