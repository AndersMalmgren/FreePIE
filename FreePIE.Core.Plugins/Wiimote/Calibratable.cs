using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class Calibratable : Subscribable
    {
        public Calibratable(out Action triggerUpdate, out Action triggerCalibrated) : base(out triggerUpdate)
        {
            triggerCalibrated = OnCalibrated;
        }

        private void OnCalibrated()
        {
            if (calibrated != null)
                calibrated();
        }

        [Documentation("Raised when the calibration is done")]
        public event Action calibrated;
    }
}