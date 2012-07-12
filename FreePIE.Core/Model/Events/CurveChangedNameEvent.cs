using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Model.Events
{
    public class CurveChangedNameEvent
    {
        public Curve Curve { get; private set; }

        public CurveChangedNameEvent(Curve curve)
        {
            Curve = curve;
        }
    }
}
