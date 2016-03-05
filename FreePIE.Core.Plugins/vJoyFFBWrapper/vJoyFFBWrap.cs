using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace vJoyFFBWrapper
{
    public static class vJoyFFBWrap
    {
        private const int IOCTL_HID_SET_FEATURE = 0xB0191,
                          IOCTL_HID_WRITE_REPORT = 0xB000F;

        private delegate void FFBCallbackWrapper(IntPtr returnedData, IntPtr userData);

        private static FFBCallbackWrapper wrapper;
        private static bool isRegistered;
        private static List<FFBCallbackFunction> callbacks = new List<FFBCallbackFunction>();

        public delegate void FFBCallbackFunction(int deviceID, PacketType packetType, IntPtr data);
        public static event FFBCallbackFunction OnFFBCallback
        {
            add
            {
                if (!isRegistered)
                {
                    wrapper = new FFBCallbackWrapper(OnWrapperCallback); //needed to keep a reference!
                    _FfbRegisterGenCB(wrapper, IntPtr.Zero);
                    isRegistered = true;
                }
                if (!callbacks.Contains(value))
                    callbacks.Add(value);
            }
            remove
            {
                if (callbacks.Contains(value))
                    callbacks.Remove(value);
                /*
				if (callbacks.Count == 0)
				{
					throw new NotImplementedException("no way to unregister from vJoy (yet?)");
					isRegistered = false;
				}*/
            }
        }
        private static Task LastTask;
        /// <summary>
        /// Reads received pointer, extracts data and passes it on to the eventhandlers
        /// </summary>
        /// <param name="ffbDataPtr"></param>
        /// <param name="userData"></param>
        private static void OnWrapperCallback(IntPtr ffbDataPtr, IntPtr userData)
        {
            //used to make sure that the blocking device calls are handled in a different thread
            if (LastTask == null)
                LastTask = Task.Factory.StartNew(() => WrappedCallback(ffbDataPtr));
            LastTask = LastTask.ContinueWith((tas) => WrappedCallback(ffbDataPtr));
        }

        private static void WrappedCallback(IntPtr ffbDataPtr)
        {
            Console.WriteLine("----------------------");
            //Very slow, according to http://www.codeproject.com/Articles/25896/Reading-Unmanaged-Data-Into-Structures. It's pretty old however, maybe PtrToStructure etc have been made faster nowadays? Besides, I haven't even checked if PtrToStructure causes an actual performance impact/bottleneck. But it'd be very easy to use an unsafe context here.
            FFBData ffbData = (FFBData)Marshal.PtrToStructure(ffbDataPtr, typeof(FFBData));
            Console.WriteLine($"DataSize: {ffbData.DataSize}, CMD: 0x{ffbData.Command:X8}");
            byte[] data = ffbData.Data;
            Console.WriteLine(data.ToHexString());

            //Convert received data to appropriate values
            int deviceID = (data[0] & 0xF0) >> 4;
            if (deviceID < 1)
                throw new Exception($"DeviceID out of range, {deviceID}");
            Console.WriteLine("Device ID: {0}", deviceID);

            PacketType packetType = (PacketType)(data[0] & 0x0F + (ffbData.Command == IOCTL_HID_SET_FEATURE ? 0x10 : 0));
            Console.WriteLine("Packet type: {0}", packetType.ToString());

            //be aware that this number might be incorrect - it isn't applicable (and thus not used) for every packet type
            /*
			int blockIndex = data[1];
			Console.WriteLine("BlockIndex: {0}", blockIndex);

			
			object returnData = null;
			switch (packetType)
			{
				case PacketType.Effect:
					EffectReport report = (EffectReport)Marshal.PtrToStructure(ffbData.PtrToData, typeof(EffectReport));
					returnData = report;
					Console.WriteLine(report);
					break;
				case PacketType.Envelope:
					break;
				case PacketType.Condition:
					break;
				case PacketType.Periodic:
					break;
				case PacketType.ConstantForce:
					ushort Magnitude = BitConverter.ToUInt16(data, 2);
					returnData = Magnitude;
					Console.WriteLine("Magnitude: {0}", Magnitude);
					break;
				case PacketType.RampForce:
					break;
				case PacketType.CustomForceData:
					break;
				case PacketType.DownloadForceSample:
					break;
				case PacketType.EffectOperation:
					break;
				case PacketType.PIDBlockFree:
					break;
				case PacketType.PIDDeviceControl:
					break;
				case PacketType.DeviceGain:
					break;
				case PacketType.SetCustomForce:
					break;
				case PacketType.CreateNewEffect:
					EffectType et = (EffectType)data[1];
					Console.WriteLine("Requested packet to create {0} effect. Will not create now, but as soon as we receive an 'Effect' packet, which contains this info (EffectType) as well as other important data", et);
					break;
				case PacketType.BlockLoad:
					break;
				case PacketType.PIDPool:
					break;
				default:
					break;
			}
			*/
            foreach (FFBCallbackFunction callback in callbacks)
            {
                callback(deviceID, packetType, ffbData.PtrToData);
            }
        }

        [DllImport("vJoyInterface.dll", EntryPoint = "FfbRegisterGenCB", CallingConvention = CallingConvention.Cdecl)]
        private extern static void _FfbRegisterGenCB(FFBCallbackWrapper callback, IntPtr data);
        /*
				[DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Packet", CallingConvention = CallingConvention.Cdecl)]
				private static extern uint _Ffb_h_Packet(IntPtr Packet, out uint Type, out int DataSize, out IntPtr Data);

				[DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_DeviceID", CallingConvention = CallingConvention.Cdecl)]
				private static extern uint _Ffb_h_DeviceID(IntPtr Packet, out int DeviceID);

				[DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Type", CallingConvention = CallingConvention.Cdecl)]
				private static extern uint _Ffb_h_Type(IntPtr Packet, out PacketType Type);
				*/

    }
}
