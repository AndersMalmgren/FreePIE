using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class AccelerationGlobal : Calibratable
    {
        private IWiimoteData data;

        public AccelerationGlobal(IWiimoteData data, out Action trigger, out Action calibrated) : base(out trigger, out calibrated)
        {
            this.data = data;
        }

        public double x { get { return data.Acceleration.Value.x; } }
        public double y { get { return data.Acceleration.Value.y; } }
        public double z { get { return data.Acceleration.Value.z; } }
        public override string ToString()
        {
            return String.Format("x: {0}, y: {1}, z: {2}", x, y, z);
        }
    }
}
