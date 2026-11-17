using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class Subscribable
    {
        public Subscribable(out Action trigger)
        {
            trigger = OnUpdate;
        }

        private void OnUpdate()
        {
            if (update != null)
                update();
        }

        [Documentation("Raised when new data has been received, assign a function with +=")]
        public event Action update;
    }
}
