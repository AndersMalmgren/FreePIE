using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.Zeiss
{
    public static class Api
    {
        public static bool Init(out int error)
        {
            var result = Tracker_Init();
            error = GetLastError();
            return result;
        }

        public static string GetError(int error)
        {
            var errorBilder = new StringBuilder();
            GetErrorString(errorBilder, error);

            return errorBilder.ToString();
        }

        [DllImport("Tracker.dll", EntryPoint = "Tracker_WaitNextFrame", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool WaitNextFrame(UInt32 waitMilliSeconds = 100);

        [DllImport("Tracker.dll", EntryPoint = "Tracker_GetFrame", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetFrame(ref Frame frame);

        [DllImport("Tracker.dll", EntryPoint = "Tracker_QuatGetEuler", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool QuatGetEuler(ref Euler euler, Quat quat);

        [DllImport("Tracker.dll", EntryPoint = "Tracker_RotateTrackerToCinemizer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RotateTrackerToCinemizer(ref Quat cinemizerQuat, Quat trackerQuat);

        [DllImport("Tracker.dll", EntryPoint = "Tracker_Release", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Release();

        [DllImport("Tracker.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Tracker_Init();
        
        [DllImport("Tracker.dll", EntryPoint = "Tracker_GetLastError", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetLastError();
        
        [DllImport("Tracker.dll", EntryPoint = "Tracker_GetErrorString", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GetErrorString(StringBuilder trackerErrorString, int trackerError);
    }
}
