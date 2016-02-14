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
        public TssType sensor_type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TssComInfo
    {
        public TssType device_type; /**< The type of 3-Space device connected through the com port. */
        public uint serial_number; /**< The serial number for the 3-Space device connected through the com port. */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string firmware_version; /**< The version of the firmware installed on the connected 3-Space device. */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string hardware_version; /**< The hardware revision and type of the connected 3-Space device. */
        public TssFirmwareCompatibility fw_compatibility; /**< Firmware compatibility level (Note level may be lower than current if no functional changes were made). */
    }
}
