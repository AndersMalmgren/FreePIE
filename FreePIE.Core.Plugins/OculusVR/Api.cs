using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.OculusVR
{
    public static class Api
    {
        [DllImport("OVRFreePIE.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int ovr_freepie_init();
        [DllImport("OVRFreePIE.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int ovr_freepie_read(out OculusVrData output);
        [DllImport("OVRFreePIE.dll")]
        private extern static int ovr_freepie_destroy();
        [DllImport("OVRFreePIE.dll")]
        private extern static int ovr_freepie_reset_orientation();

        public static bool Init()
        {
            return ovr_freepie_init() == 0;
        }

        public static OculusVrData Read()
        {
            OculusVrData output;
            ovr_freepie_read(out output);
            return output;
        }

        public static bool Dispose()
        {
            return ovr_freepie_destroy() == 0;
        }

        public static bool Center()
        {
            return ovr_freepie_reset_orientation() == 0;
        }
    }
}
