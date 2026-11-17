using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class Nunchuck
    {
        public Acceleration Acceleration { get; set; }

        public AnalogStick Stick { get; set; }

        public NunchuckButtons Buttons { get; set; }
    }

    [Documentation("Acceleration in m/s2")]
    public class Acceleration
    {
        public Acceleration(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [Documentation("Acceleration along the x axis in m/s2")]
        public double x { get; private set; }
        [Documentation("Acceleration along the y axis in m/s2")]
        public double y { get; private set; }
        [Documentation("Acceleration along the z axis in m/s2")]
        public double z { get; private set; }
        public override string ToString()
        {
            return String.Format("x: {0}, y: {1}, z: {2}", x, y, z);
        }
    }
}
