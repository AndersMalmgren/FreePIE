using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.VJoy
{
    public static class Api
    {
        [DllImport("VJoy.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VJoy_Initialize(StringBuilder name, StringBuilder serial);

        [DllImport("VJoy.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void VJoy_Shutdown();

        [DllImport("VJoy.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VJoy_UpdateJoyState(int id, ref VJoyState joyState);

        public static bool Initialize()
        {
            return VJoy_Initialize(new StringBuilder(string.Empty), new StringBuilder(string.Empty));
        }

        public static void Dispose()
        {
            VJoy_Shutdown();
        }

        public static bool Update(int id, VJoyState state)
        {
            return VJoy_UpdateJoyState(id, ref state);
        }
    }
}
