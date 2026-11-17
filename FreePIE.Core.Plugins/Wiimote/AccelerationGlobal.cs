using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Wiimote
{
    [Documentation("Calibrated acceleration in m/s2")]
    public class AccelerationGlobal : Calibratable
    {
        private IWiimoteData data;

        public AccelerationGlobal(IWiimoteData data, out Action trigger, out Action calibrated) : base(out trigger, out calibrated)
        {
            this.data = data;
        }

        [Documentation("Acceleration along the x axis in m/s2")]
        public double x { get { return data.Acceleration.Value.x; } }
        [Documentation("Acceleration along the y axis in m/s2")]
        public double y { get { return data.Acceleration.Value.y; } }
        [Documentation("Acceleration along the z axis in m/s2")]
        public double z { get { return data.Acceleration.Value.z; } }
        public override string ToString()
        {
            return String.Format("x: {0}, y: {1}, z: {2}", x, y, z);
        }
    }
}
