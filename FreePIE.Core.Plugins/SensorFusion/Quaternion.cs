using System;

namespace FreePIE.Core.Plugins.SensorFusion
{
    public class Quaternion
    {
        public double Yaw { get; private set; }
        public double Roll { get; private set; }
        public double Pitch { get; private set; }

        public void Update(double w, double x, double y, double z)
        {
            // normalize the vector
            double len = Math.Sqrt((w * w) + (x * x) + (y * y) + (z * z));
            w /= len;
            x /= len;
            y /= len;
            z /= len;

            // Convert to angles in radians
            double m11 = (2.0f * w * w) + (2.0f * x * x) - 1.0f;
            double m12 = (2.0f * x * y) + (2.0f * w * z);
            double m13 = (2.0f * x * z) - (2.0f * w * y);
            double m23 = (2.0f * y * z) + (2.0f * w * x);
            double m33 = (2.0f * w * w) + (2.0f * z * z) - 1.0f;

            Roll = Math.Atan2(m23, m33);
            Pitch = Math.Asin(-m13);
            Yaw = Math.Atan2(m12, m11);
        }
    }
}
