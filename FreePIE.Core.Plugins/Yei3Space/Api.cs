using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FreePIE.Core.Plugins.SensorFusion;

namespace FreePIE.Core.Plugins.Yei3Space
{
    public static class Api
    {
        public static IList<TssComPort> GetComPorts()
        {
            var ports = new TssComPort[20];
            var count = tss_getComPorts(ports, (uint)ports.Length, 0, TssFind.TSS_FIND_ALL_KNOWN | TssFind.TSS_FIND_DNG);

            return ports.Take(count).ToList();
        }

        public static int CreateDevice(TssComPort port)
        {
            var deviceId = tss_createTSDeviceStr(port.Port, TssTimestampMode.TSS_TIMESTAMP_SENSOR);
            return (int)deviceId;
        }

        public static TssError UpdateQuaternion(int deviceId, Quaternion quaternion)
        {
            uint timestamp;
            var quat = new float[4];
            var error = tss_getTaredOrientationAsQuaternion((uint)deviceId, quat, out timestamp); //xyzw
            quaternion.Udate(quat[3], quat[0], quat[1], quat[2]);
            return error;
        }

        public static TssError CloseDevice(int deviceId)
        {
            return tss_closeTSDevice((uint)deviceId);
        }

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_closeTSDevice(uint device);
            
        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tss_getComPorts(TssComPort[] ports, uint size, int offset, TssFind filter);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern uint tss_createTSDeviceStr(string comport, TssTimestampMode mode);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getTaredOrientationAsQuaternion(uint deviceId, float[] quat,
                                                                            out uint timestamp);
    }
}
