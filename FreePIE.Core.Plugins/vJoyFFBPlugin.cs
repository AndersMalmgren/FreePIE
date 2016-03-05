using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using vJoyFFBWrapper;

namespace FreePIE.Core.Plugins
{
    public partial class VJoyGlobalHolder
    {
        public List<Device> RegisteredFFBDevices = new List<Device>();
        private static bool isWrapperRegistered = false;

        public bool FFBEnabled { get { return joystick.IsDeviceFfb(Index); } }

        public void RegisterFFBDevice(Device dev)
        {
            if (!FFBEnabled) throw new NotSupportedException("This VJoy device does not have FFB enabled");
            if (!isWrapperRegistered)
            {
                vJoyFFBWrap.OnFFBCallback += VJoyFFBWrap_OnFFBCallback;
                isWrapperRegistered = true;
            }
            RegisteredFFBDevices.Add(dev);
        }



        private void VJoyFFBWrap_OnFFBCallback(int deviceID, PacketType packetType, IntPtr dataPtr)
        {
            if (Index != deviceID)
                return;
            byte blockIndex = Marshal.ReadByte(dataPtr, 1);
            Console.WriteLine("BlockIndex: {0}", blockIndex);
            switch (packetType)
            {
                case PacketType.Effect:
                    EffectReport report = (EffectReport)Marshal.PtrToStructure(dataPtr, typeof(EffectReport));
                    Console.WriteLine(report);
                    RegisteredFFBDevices.ForEach(dev => dev.CreateEffect(report));
                    break;
                /*case PacketType.Envelope:
					break;
				case PacketType.Condition:
					break;
				case PacketType.Periodic:
					break;*/
                case PacketType.ConstantForce:
                    short Magnitude = Marshal.ReadInt16(dataPtr, 2);
                    Console.WriteLine("Magnitude: {0}", Magnitude);
                    RegisteredFFBDevices.ForEach(dev => dev.SetConstantForce(blockIndex, Magnitude));
                    break;
                /*case PacketType.RampForce:
					break;
				case PacketType.CustomForceData:
					break;
				case PacketType.DownloadForceSample:
					break;*/
                case PacketType.EffectOperation:
                    EffectOperation effOp = (EffectOperation)Marshal.ReadByte(dataPtr, 2);
                    Console.WriteLine("EffectOperation: {0}", effOp);
                    RegisteredFFBDevices.ForEach(dev => dev.OperateEffect(blockIndex, effOp, Marshal.ReadByte(dataPtr, 3)));
                    break;
                case PacketType.PIDBlockFree:
                    Console.WriteLine("PID: Freeing block {0}", blockIndex);
                    RegisteredFFBDevices.ForEach(dev => dev.DisposeEffect(blockIndex));
                    break;
                case PacketType.PIDDeviceControl:
                    Console.WriteLine("PIDDeviceControl: {0}", (PIDDeviceControl)Marshal.ReadByte(dataPtr, 1));
                    break;
                /*
            case PacketType.DeviceGain:
                break;
            case PacketType.SetCustomForce:
                break;
            case PacketType.CreateNewEffect:
                break;
            case PacketType.BlockLoad:
                break;
            case PacketType.PIDPool:
                break;*/
                default:
                    Console.WriteLine("WARNING!!!!!! PACKET NOT HANDLED: {0}", packetType);
                    break;
            }
        }
    }
}
