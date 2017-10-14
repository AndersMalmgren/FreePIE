using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class ClassicController
    {
        public AnalogStick RightStick { get; set; }
        public AnalogStick LeftStick { get; set; }
        public AnalogTrigger LeftTrigger { get; set; }
        public AnalogTrigger RightTrigger { get; set; }
    }
}
