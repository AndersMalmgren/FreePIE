using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FreePIE.Core.Plugins.SensorFusion;

namespace FreePIE.Core.Plugins.Yei3Space
{
    public static class Api
    {
        public static bool is_streaming;

        public static int GetComPorts()
        {
            TssComPort port = new TssComPort();
            var count = tss_getComPorts(out port, (uint)1, 0, TssFind.TSS_FIND_ALL_KNOWN);
            return count;
        }

        public static int CreateDevice(int index)
        {
            int serial;
            uint device_id = 0;
            TssComPort[] ports = new TssComPort[20];
            var count = tss_getComPorts(ports, (uint)20, 0, TssFind.TSS_FIND_ALL_KNOWN);

            //I am assuming that sensor type 4 is a dongle. Someone will have to test this who have the wireless version
            if (ports[index].sensor_type == 4)
            {
                Console.WriteLine("TSS:Device is wireless - trying to find matching sensor");
                uint dongle_id = tss_createTSDeviceStr(ports[index].com_port, TssTimestampMode.TSS_TIMESTAMP_NONE);
                //tss_getSerialNumber(dongle_id, serial);
                for (int i = 0; i < 15; i++)
                {
                    device_id = 0x800000;
                    Console.WriteLine("TSS:Checking dongle index " + i);
                    tss_getSensorFromDongle((int)dongle_id, i, out device_id);
                    if ((TssDeviceIdMask)device_id != TssDeviceIdMask.TSS_NO_DEVICE_ID)
                    {
                        tss_getSerialNumber((int)device_id, out serial);
                        Console.WriteLine("Wireless Sensor Found, serial: " + serial.ToString("X") + " on space: " + i);
                        //StartStreamingU((int)device_id);
                        break;
                    }
                }
            }
            else
            {
                device_id = tss_createTSDeviceStr(ports[index].com_port, TssTimestampMode.TSS_TIMESTAMP_NONE);
                if ((TssDeviceIdMask)device_id != TssDeviceIdMask.TSS_NO_DEVICE_ID)
                {
                    tss_getSerialNumber((int)device_id, out serial);
                    Console.WriteLine("TSS:Created, device serial is " + serial.ToString("X"));
                    //StartStreamingU((int)device_id);
                }
            }

            return (int)device_id;
        }

        public static TssError UpdateQuaternion(int deviceId, Quaternion quaternion)
        {
            uint timestamp;
            float[] quat = new float[4];
            TssStreamPacket packet;
            TssError error;
            if (is_streaming)
            {
                error = tss_getLastStreamData(deviceId, out packet, 16);
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
        public static void StartStreamingU(int device_id)
        {
            int count;
            byte[] stream_slots = new byte[8];
            stream_slots[0] = (byte)TssStreaming.TSS_GET_TARED_ORIENTATION_AS_QUATERNION;
            stream_slots[1] = (byte)TssStreaming.TSS_NULL;
            stream_slots[2] = (byte)TssStreaming.TSS_NULL;
            stream_slots[3] = (byte)TssStreaming.TSS_NULL;
            stream_slots[4] = (byte)TssStreaming.TSS_NULL;
            stream_slots[5] = (byte)TssStreaming.TSS_NULL;
            stream_slots[6] = (byte)TssStreaming.TSS_NULL;
            stream_slots[7] = (byte)TssStreaming.TSS_NULL;

            count = 0;
            if (!is_streaming)
            {
                while (count < 3)
                {
                    if (tss_setStreamingTiming(device_id, 0, -1, 1500000) == 0)
                    {
                        if (tss_setStreamingSlots(device_id, stream_slots) == 0)
                        {
                            if (tss_startStreaming(device_id) == 0)
                            {
                                is_streaming = true;
                                break;
                            }
                        }
                    }
                    count++;
                }
            }

            if (!is_streaming)
            {
                Console.WriteLine("Failed to Start Streaming!");
            }
        }

        // Stops streaming asynchronous data
        public static void StopStreamingU(int device_id)
        {
            int count;
            count = 0;
            if (is_streaming)
            {
                while (count < 3)
                {
                    if (tss_stopStreaming(device_id) == 0)
                    {
                        is_streaming = false;
                        break;
                    }
                    count++;
                }
            }

            if (is_streaming)
            {
                Console.WriteLine("Failed to Stop Streaming!");
            }
        }

        public static void StopStreaming(int device_id)
        {
            if (is_streaming)
            {
                StopStreamingU(device_id);
                is_streaming = false;
                Console.WriteLine("Streaming Stopped");
            }
        }

        public static void StartStreaming(int device_id)
        {
            if (!is_streaming)
            {
                StartStreamingU(device_id);
                is_streaming = true;
                Console.WriteLine("Streaming Restarted");
            }
        }

        public static TssError CloseDevice(int deviceId)
        {
            if (is_streaming)
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
        private static extern TssError tss_getLastStreamData(int id, out TssStreamPacket packet, int size);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern TssDeviceIdMask tss_getDeviceType(int id, out TssDeviceIdMask device_type);

        [DllImport("ThreeSpace_API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tss_getSensorFromDongle(int id, int logical_id, out uint w_ts_device);             
    }
}
