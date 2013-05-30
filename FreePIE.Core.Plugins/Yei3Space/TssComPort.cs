using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.Yei3Space
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TssComPort
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string com_port;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string friendly_name;
        public int sensor_type;
    }
}
