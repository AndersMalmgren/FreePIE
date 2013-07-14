using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers.Strategies
{
    internal class ContinuesRotationStrategy
    {
        private readonly Units unit;
        private double previousSample;

        public ContinuesRotationStrategy(Units unit)
        {
            this.unit = unit;
        }

        public void Update(double x)
        {
            double delta = x - previousSample;

            double halfCircle = unit == Units.Radians ? Math.PI : 180d;

            if (Math.Abs(delta) > halfCircle)
            {
                if (delta > 0)
                    delta -= (2*halfCircle);
                else
                    delta += (2*halfCircle);
            }
            Out += delta;
            previousSample = x;
        }

        public double Out { get; private set; }

    }
}
