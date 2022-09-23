using System;
using System.Runtime.InteropServices;

namespace JonesCorp
{
    public class WinUsbWrapper
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WINUSB_SETUP_PACKET
        {
            internal byte RequestType;
            internal byte Request;
            internal UInt16 Value;
            internal UInt16 Index;
            internal UInt16 Length;
        }

        public enum USBD_PIPE_TYPE
        {
            UsbdPipeTypeControl = 0,
            UsbdPipeTypeIsochronous = 1,
            UsbdPipeTypeBulk = 2,
            UsbdPipeTypeInterrupt = 3,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_INTERFACE_DESCRIPTOR
        {
            internal byte bLength;
            internal byte bDescriptorType;
            internal byte bInterfaceNumber;
            internal byte bAlternateSetting;
            internal byte bNumEndpoints;
            internal byte bInterfaceClass;
            internal byte bInterfaceSubClass;
            internal byte bInterfaceProtocol;
            internal byte iInterface;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINUSB_PIPE_INFORMATION
        {
            internal USBD_PIPE_TYPE PipeType;
            internal byte PipeId;
            internal UInt16 MaximumPacketSize;
            internal byte Interval;
        }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff540277%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="DeviceHandle">The handle to the device that CreateFile returned. WinUSB uses overlapped I/O, so FILE_FLAG_OVERLAPPED must be specified in the dwFlagsAndAttributes parameter of CreateFile call for DeviceHandle to have the characteristics necessary for WinUsb_Initialize to function properly.</param>
        /// <param name="InterfaceHandle">Receives an opaque handle to the first (default) interface on the device. This handle is required by other WinUSB routines that perform operations on the default interface. To release the handle, call the <see cref="WinUsb_Free"/>function.</param>
        /// <returns></returns>
        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_Initialize(IntPtr DeviceHandle, ref IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_QueryInterfaceSettings(IntPtr InterfaceHandle, byte AlternateInterfaceNumber, ref USB_INTERFACE_DESCRIPTOR UsbAltInterfaceDescriptor);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_QueryPipe(IntPtr InterfaceHandle, byte AlternateInterfaceNumber, byte PipeIndex, ref WINUSB_PIPE_INFORMATION PipeInformation);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_AbortPipe(IntPtr InterfaceHandle, byte PipeID);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_FlushPipe(IntPtr InterfaceHandle, byte PipeID);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket, byte[] Buffer, int BufferLength, ref int LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_ReadPipe(IntPtr InterfaceHandle, byte PipeID, byte[] Buffer, int BufferLength, ref int LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_WritePipe(IntPtr InterfaceHandle, byte PipeID, byte[] Buffer, int BufferLength, ref int LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_Free(IntPtr InterfaceHandle);
    }
}
