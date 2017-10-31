using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class StatusGlobal : Subscribable
    {
        private IWiimoteData data;

        public StatusGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
        }

        public int batteryPercentage
        {
            get
            {
                return data.BatteryPercentage;
            }
        }
    }
}
