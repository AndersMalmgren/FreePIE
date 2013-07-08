using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.VJoy
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct VJoyState
    {
        public byte ReportId;
        public short XAxis;
        public short YAxis;
        public short ZAxis;
        public short XRotation;
        public short YRotation;
        public short ZRotation;
        public short Slider;
        public short Dial;
        public ushort POV;
        public uint Buttons;
    };
}
