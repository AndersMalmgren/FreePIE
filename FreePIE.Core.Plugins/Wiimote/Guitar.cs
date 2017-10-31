using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class Guitar
    {
        public AnalogStick Stick { get; set; }
        public TapBar TapBar { get; set; }
        public AnalogTrigger Whammy { get; set; }
        public Boolean IsGH3 { get; set; }
        public GuitarButtons Buttons { get; set; }
    }
    public class TapBar
    {
        private double val;
        public TapBar(int val)
        {
            this.val = val;
        }
        public Boolean isGreen()
        {
            return val >= 0x04 && val < 0x0A;
        }
        public Boolean isRed()
        {
            return val >= 0x07 && val < 0x12;
        }
        public Boolean isYellow()
        {
            return val >= 0x0C && val < 0x17;
        }
        public Boolean isBlue()
        {
            return val >= 0x14 && val < 0x18;
        }
        public Boolean isOrange()
        {
            return val >= 0x1A && val < 0x1F;
        }
        public override string ToString()
        {
            return String.Format("green: {0}, red {1}, yellow {2}, blue {3}, orange {4}", isGreen(), isRed(), isYellow(), isBlue(), isOrange());
        }
    }
}
