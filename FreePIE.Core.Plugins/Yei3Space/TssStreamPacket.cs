using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.Yei3Space
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TssStreamPacketQuat
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] quat;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TssStreamPacketQuatButton
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] quat;
        public byte button_state;
    }
}
