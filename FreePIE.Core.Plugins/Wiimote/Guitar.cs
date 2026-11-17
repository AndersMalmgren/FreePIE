using System;
using FreePIE.Core.Contracts;

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
    [Documentation("The Guitar Hero touch bar")]
    public class TapBar
    {
        private double val;
        public TapBar(int val)
        {
            this.val = val;
        }
        [Documentation("True while the green part of the bar is touched")]
        public Boolean isGreen()
        {
            return val >= 0x04 && val < 0x0A;
        }
        [Documentation("True while the red part of the bar is touched")]
        public Boolean isRed()
        {
            return val >= 0x07 && val < 0x12;
        }
        [Documentation("True while the yellow part of the bar is touched")]
        public Boolean isYellow()
        {
            return val >= 0x0C && val < 0x17;
        }
        [Documentation("True while the blue part of the bar is touched")]
        public Boolean isBlue()
        {
            return val >= 0x14 && val < 0x18;
        }
        [Documentation("True while the orange part of the bar is touched")]
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
