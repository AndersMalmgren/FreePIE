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
        private extern static int ovr_freepie_init(float sensorPrediction);
        [DllImport("OVRFreePIE.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int ovr_freepie_read(out OculusVr3Dof output);
        [DllImport("OVRFreePIE.dll")]
        private extern static int ovr_freepie_destroy();
        [DllImport("OVRFreePIE.dll")]
        private extern static int ovr_freepie_reset_orientation();

        public static bool Init(float sensorPrediction)
        {
            return ovr_freepie_init(sensorPrediction) == 0;
        }

        public static OculusVr3Dof Read()
        {
            OculusVr3Dof output;
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
