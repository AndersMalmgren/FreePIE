using System.Runtime.InteropServices;
using FreePIE.Core.Plugins.SensorFusion;

namespace FreePIE.Core.Plugins.Yei3Space
{
    public static class Api
    {
        private static bool isStreaming;

        public static int GetComPorts()
        {
            var port = new TssComPort();
            var count = tss_getComPorts(out port, 1, 0, TssFind.TSS_FIND_ALL_KNOWN);
            return count;
        }

        public static int CreateDevice(int index)
        {
            int serial;
            uint deviceId = 0;
            var ports = new TssComPort[20];
            tss_getComPorts(ports, 20, 0, TssFind.TSS_FIND_ALL_KNOWN);

            //I am assuming that sensor type 4 is a dongle. Someone will have to test this who have the wireless version
            if (ports[index].sensor_type == 4)
            {
                uint dongle_id = tss_createTSDeviceStr(ports[index].com_port, TssTimestampMode.TSS_TIMESTAMP_NONE);
                for (int i = 0; i < 15; i++)
                {
                    tss_getSensorFromDongle((int)dongle_id, i, out deviceId);
                    if ((TssDeviceIdMask)deviceId != TssDeviceIdMask.TSS_NO_DEVICE_ID)
                    {
                        tss_getSerialNumber((int)deviceId, out serial);
                        StartStreamingU((int)deviceId);
                        break;
                    }
                }
            }
            else
            {
                deviceId = tss_createTSDeviceStr(ports[index].com_port, TssTimestampMode.TSS_TIMESTAMP_NONE);
                if ((TssDeviceIdMask)deviceId != TssDeviceIdMask.TSS_NO_DEVICE_ID)
                {
                    tss_getSerialNumber((int)deviceId, out serial);
                    StartStreamingU((int)deviceId);
                }
            }

            return (int)deviceId;
        }

        public static TssError UpdateQuaternion(int deviceId, Quaternion quaternion)
        {
            uint timestamp;
            var quat = new float[4];
            TssStreamPacket packet;
            TssError error;
            if (isStreaming)
            {
                error = tss_getLastStreamData(deviceId, out packet, 16, out timestamp);
                quat[0] = packet.quat[0];
                quat[1] = packet.quat[1];
                quat[2] = packet.quat[2];
                quat[3] = packet.quat[3];
            }
            else
            {
                error = tss_getTaredOrientationAsQuaternion((uint)deviceId, quat, out timestamp); //xyzw                                
            }
            quaternion.Update(quat[3], quat[0], quat[1], quat[2], false);

            return error;
        }

        // Starts streaming asynchronous data
        private static void StartStreamingU(int device_id)
        {
            int count;
            byte[] streamSlots = new byte[8];
            streamSlots[0] = (byte)TssStreaming.TSS_GET_TARED_ORIENTATION_AS_QUATERNION;
            streamSlots[1] = (byte)TssStreaming.TSS_NULL;
            streamSlots[2] = (byte)TssStreaming.TSS_NULL;
            streamSlots[3] = (byte)TssStreaming.TSS_NULL;
            streamSlots[4] = (byte)TssStreaming.TSS_NULL;
            streamSlots[5] = (byte)TssStreaming.TSS_NULL;
            streamSlots[6] = (byte)TssStreaming.TSS_NULL;
            streamSlots[7] = (byte)TssStreaming.TSS_NULL;

            count = 0;
            if (!isStreaming)
            {
                while (count < 3)
                {
                    if (tss_setStreamingTiming(device_id, 0, -1, 1500000) == 0)
                    {
                        if (tss_setStreamingSlots(device_id, streamSlots) == 0)
                        {
                            if (tss_startStreaming(device_id) == 0)
                            {
                                isStreaming = true;
                                break;
                            }
                        }
                    }
                    count++;
                }
            }
        }

        // Stops streaming asynchronous data
        private static void StopStreamingU(int device_id)
        {
            int count;
            count = 0;
            if (isStreaming)
            {
                while (count < 3)
                {
                    if (tss_stopStreaming(device_id) == 0)
                    {
                        isStreaming = false;
                        break;
                    }
                    count++;
                }
            }
        }

        public static void StopStreaming(int device_id)
        {
            if (isStreaming)
            {
                StopStreamingU(device_id);
                isStreaming = false;
            }
        }

        public static void StartStreaming(int device_id)
        {
            if (!isStreaming)
            {
                StartStreamingU(device_id);
                isStreaming = true;
            }
        }

        public static TssError TareSensor(int device_id)
        {
            return tss_tareWithCurrentOrientation(device_id);
        }

        public static TssError CloseDevice(int deviceId)
        {
            if (isStreaming)
            {
                StopStreamingU(deviceId);
            }
            return tss_closeTSDevice((uint)deviceId);
        }

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_closeTSDevice(uint device);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tss_getSerialNumber(int id, out int serial);

        //Method for calling just one sesnsor (the first presumably)
        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tss_getComPorts(out TssComPort port, uint size, int offset, TssFind filter);

        //Method for getting all comports
        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tss_getComPorts([Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] TssComPort[] ports, uint size, int offset, TssFind filter);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getUntaredOrientationAsQuaternion(uint deviceId, float[] quat);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern uint tss_createTSDeviceStr(string comport, TssTimestampMode mode);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getTaredOrientationAsQuaternion(uint deviceId, float[] quat,
                                                                            out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_tareWithCurrentOrientation(int deviceId);        
        
        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_tareWithQuaternion(int deviceId, float[] quat);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_setStreamingTiming(int id, int interval, int duration, int delay);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_setStreamingSlots(int id, byte[] stream_slots);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_startStreaming(int id);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_stopStreaming(int id);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getLastStreamData(int id, out TssStreamPacket packet, int size, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssDeviceIdMask tss_getDeviceType(int id, out TssDeviceIdMask device_type);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tss_getSensorFromDongle(int id, int logical_id, out uint w_ts_device);             
    }
}
