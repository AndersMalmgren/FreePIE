using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Strategies
{
    public enum Units
    {
        AutoDetect = 1,
        Radians = 2,
        Degrees = 4
    }

    public class ContinousYawStrategy
    {
        private Units unit;
        private double previousYawSample;
        private int autoDetectSampleCount;
        
        public ContinousYawStrategy(Units unit)
        {
            this.unit = unit;
        }

        public void Update(double yawSample)
        {
            if (Enable)
            {
                AutoDetectUnits(yawSample);

                double delta = yawSample - previousYawSample;

                double halfCircle = unit == Units.Radians ? Math.PI : 180d;

                if (Math.Abs(delta) > halfCircle)
                {
                    if (delta > 0)
                        delta -= (2*halfCircle);
                    else
                        delta += (2*halfCircle);
                }
                Yaw += delta;
                previousYawSample = yawSample;
            }
            else
            {
                Yaw = yawSample;
            }
        }

        private void AutoDetectUnits(double yawSample)
        {
            if (unit == Units.AutoDetect)
            {
                if (autoDetectSampleCount > 50)
                {
                    unit = Units.Radians;
                }
                else if (Math.Abs(yawSample) > Math.PI*2)
                {
                    unit = Units.Degrees;
                }
                autoDetectSampleCount++;
            }
        }

        public bool Enable
        {
            get; set;
        }

        public double Yaw { get; private set; }

        public ContinousYawStrategy() : this(Units.AutoDetect) {} 
    }
}
