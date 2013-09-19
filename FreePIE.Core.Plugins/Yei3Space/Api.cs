using System.Runtime.InteropServices;
using FreePIE.Core.Plugins.SensorFusion;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins.Yei3Space
{
    public struct SerialInfo
    {
        public string com_port;
        public byte logical_id;
    }

    public static class Api
    {
        public static uint[] device_serials= new uint[6];
        public static bool stream_button_state;
        public static bool poll_unknown_devices;
        public static SortedDictionary<uint, SerialInfo> serial_info = new SortedDictionary<uint, SerialInfo>();
        public static List<uint> found_serials = new List<uint>();
        public static List<uint> active_dongles = new List<uint>();
      
         
        public static int GetComPorts()
        {
            serial_info.Clear();
            var ports = new TssComPort[20];
            TssFind search;
            if (poll_unknown_devices)
            {
                search = TssFind.TSS_FIND_ALL;
            }
            else
            {
                search = TssFind.TSS_FIND_ALL_KNOWN;
            }
            var count = tss_getComPorts(ports, 20, 0, search);
            TssComInfo com_info = new TssComInfo();
            for (int i = 0; i < count; i++)
            {
                if( tss_getTSDeviceInfoFromComPort(ports[i].com_port, out com_info) == TssError.TSS_NO_ERROR)
                {
                    if (com_info.device_type == TssType.TSS_DNG)
                    {
                        count--; //dongle itself is not a device that can stream
                        uint dongle_id = (uint)TssDeviceIdMask.TSS_NO_DEVICE_ID;
                        dongle_id = tss_createTSDeviceStr(ports[i].com_port, TssTimestampMode.TSS_TIMESTAMP_NONE);
                        active_dongles.Add(dongle_id);
                        for (byte j = 0; j < 15; j++)
                        {
                            uint serial_number, timestamp;
                            tss_getSerialNumberAtLogicalID(dongle_id, j, out serial_number, out timestamp);
                            if (serial_number != 0)
                            {
                                SerialInfo info = new SerialInfo();
                                info.com_port = ports[i].com_port;
                                info.logical_id = j;
                                serial_info[serial_number] = info;
                                count++;
                            }
                        }
                    }
                    else
                    {
                        SerialInfo info = new SerialInfo();
                        info.com_port = ports[i].com_port;
                        info.logical_id = 0xff;
                        serial_info[com_info.serial_number] = info;
                    }
                }
            }
            found_serials = new List<uint>(serial_info.Keys);
            return count;
        }

        public static uint CreateDevice(int index)
        {
            SerialInfo info;
            uint serial_number;
            uint deviceId = (uint)TssDeviceIdMask.TSS_NO_DEVICE_ID;
            if (index > device_serials.Length || device_serials[index] == 0)
            {
                info = serial_info[found_serials[index]];
                serial_number = found_serials[index];
            }
            else
            {
                info = serial_info[device_serials[index]];
                serial_number = device_serials[index];
            }

            if (info.logical_id != 0xff)
            {
                foreach (var dongle in active_dongles)
                {
                    uint rtn_serial, timestamp;
                    tss_getSerialNumberAtLogicalID(dongle, info.logical_id, out rtn_serial, out timestamp);
                    if (serial_number == rtn_serial)
                    {
                        tss_getSensorFromDongle(dongle, info.logical_id, out deviceId);
                        if ((TssDeviceIdMask)deviceId != TssDeviceIdMask.TSS_NO_DEVICE_ID)
                        {
                            StartStreamingU(deviceId);
                            return deviceId;
                        }
                        throw new System.Exception("Failed to create deviceId from wireless sensor");
                    }
                }
                throw new System.Exception("Serial not found in connected dongles");
            }
            deviceId = tss_createTSDeviceStr(info.com_port, TssTimestampMode.TSS_TIMESTAMP_NONE);
            if ((TssDeviceIdMask)deviceId != TssDeviceIdMask.TSS_NO_DEVICE_ID)
            {
                StartStreamingU(deviceId);
                return deviceId;
            }
            throw new System.Exception("Device Not Found");
        }

        public static TssError UpdateSensor(uint deviceId, Quaternion quaternion, out byte button_state)
        {
            uint timestamp;
            var quat = new float[4];
            TssError error;
            if (stream_button_state)
            {
                TssStreamPacketQuatButton packet;
                error = tss_getLastStreamData(deviceId, out packet, 17, out timestamp);
                quat[0] = packet.quat[0];
                quat[1] = packet.quat[1];
                quat[2] = packet.quat[2];
                quat[3] = packet.quat[3];
                button_state = packet.button_state;
            }
            else
            {
                TssStreamPacketQuat packet;
                error = tss_getLastStreamData(deviceId, out packet, 16, out timestamp);
                quat[0] = packet.quat[0];
                quat[1] = packet.quat[1];
                quat[2] = packet.quat[2];
                quat[3] = packet.quat[3];
                button_state = 0;
            }
            quaternion.Update(quat[3], quat[0], quat[1], quat[2], false);

            return error;
        }

        // Starts streaming asynchronous data
        private static void StartStreamingU(uint device_id)
        {
            int count;
            uint timestamp;
            byte[] streamSlots = new byte[8];
            streamSlots[0] = (byte)TssStreaming.TSS_GET_TARED_ORIENTATION_AS_QUATERNION;
            if (stream_button_state)
            {
                streamSlots[1] = (byte)TssStreaming.TSS_GET_BUTTON_STATE;
            }
            else
            {
                streamSlots[1] = (byte)TssStreaming.TSS_NULL;
            }
            streamSlots[2] = (byte)TssStreaming.TSS_NULL;
            streamSlots[3] = (byte)TssStreaming.TSS_NULL;
            streamSlots[4] = (byte)TssStreaming.TSS_NULL;
            streamSlots[5] = (byte)TssStreaming.TSS_NULL;
            streamSlots[6] = (byte)TssStreaming.TSS_NULL;
            streamSlots[7] = (byte)TssStreaming.TSS_NULL;

            count = 0;
            while (count < 3)
            {
                if (tss_setStreamingTiming(device_id, 0, -1, 1500000, out timestamp) == 0)
                {
                    if (tss_setStreamingSlots(device_id, streamSlots, out timestamp) == 0)
                    {
                        if (tss_startStreaming(device_id, out timestamp) == 0)
                        {
                            //isStreaming = true;
                            break;
                        }
                    }
                }
                count++;
            }
        }

        // Stops streaming asynchronous data
        private static void StopStreamingU(uint device_id)
        {
            uint timestamp;
            var error = tss_stopStreaming(device_id, out timestamp);
            if ( error != 0)
            {
                throw new System.Exception(string.Format("device_id {0} failed to stop", device_id));
            }

        }

        public static void StopStreaming(uint device_id)
        {
                StopStreamingU(device_id);
        }

        public static void StartStreaming(uint device_id)
        {
                StartStreamingU(device_id);
        }

        public static TssError TareSensor(uint device_id)
        {
            uint timestamp;
            return tss_tareWithCurrentOrientation(device_id, out timestamp);
        }

        public static TssError CloseDevice(uint deviceId)
        {
            StopStreamingU(deviceId);
            return tss_closeTSDevice(deviceId);
        }

        public static void CloseDongles()
        {
            foreach (var dongle in active_dongles)
            {
                tss_closeTSDevice(dongle);
            }
            active_dongles.Clear();
        }



        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_closeTSDevice(uint device);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getSerialNumber(uint device, out uint serial_number, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getSerialNumberAtLogicalID(uint device, byte logical_id, out uint serial_number, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern TssError tss_getTSDeviceInfoFromComPort(string comport, out TssComInfo com_info);

        //Method for calling just one sesnsor (the first presumably)
        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tss_getComPorts(out TssComPort port, uint size, int offset, TssFind filter);

        //Method for getting all comports
        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tss_getComPorts([Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] TssComPort[] ports, uint size, int offset, TssFind filter);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getUntaredOrientationAsQuaternion(uint deviceId, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] out float[] quat, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern uint tss_createTSDeviceStr(string comport, TssTimestampMode mode);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getTaredOrientationAsQuaternion(uint deviceId, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)]out float[] quat,
                                                                            out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_tareWithCurrentOrientation(uint device, out uint timestamp);        
        
        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_tareWithQuaternion(uint device, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] float[] quat, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_setStreamingTiming(uint device, int interval, int duration, int delay, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_setStreamingSlots(uint device, [MarshalAs(UnmanagedType.LPArray, SizeConst = 8)] byte[] stream_slots, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_startStreaming(uint device, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_stopStreaming(uint device, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getLastStreamData(uint device, out TssStreamPacketQuat packet, int size, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getLastStreamData(uint device, out TssStreamPacketQuatButton packet, int size, out uint timestamp);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getTSDeviceType(uint device, out TssType device_type);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssError tss_getSensorFromDongle(uint device, int logical_id, out uint w_ts_device);             
    }
}
