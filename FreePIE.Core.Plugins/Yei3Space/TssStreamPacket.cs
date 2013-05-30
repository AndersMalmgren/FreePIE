using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.Yei3Space
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TssStreamPacket
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] quat;
    }
}
