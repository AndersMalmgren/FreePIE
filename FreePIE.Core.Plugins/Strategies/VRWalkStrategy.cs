using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Strategies
{
    public class VRWalkStrategy
    {
        private const double deltaAngle = 270*Math.PI / 180;

        public void Update(double delta, double bearing, double yaw)
        {
            var aXY = yaw - deltaAngle - bearing;
            DeltaX = delta * Math.Cos(aXY);
            DeltaY = delta * Math.Sin(aXY);
        }

        public double DeltaX { get; private set; }
        public double DeltaY { get; private set; }
    }
}
