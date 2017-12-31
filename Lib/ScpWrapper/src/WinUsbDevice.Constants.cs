using System;
using System.Runtime.InteropServices;

namespace JonesCorp
{
    public partial class WinUsbDevice
    {
        protected const int INVALID_HANDLE_VALUE = -1;

        protected const UInt32 FILE_ATTRIBUTE_NORMAL = 0x80;
        protected const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
        protected const UInt32 FILE_SHARE_READ = 1;
        protected const UInt32 FILE_SHARE_WRITE = 2;
        protected const UInt32 GENERIC_READ = 0x80000000;
        protected const UInt32 GENERIC_WRITE = 0x40000000;

        public enum SericeNotificationType
        {
            SERVICE_CONTROL_STOP = 0x00000001,
            SERVICE_CONTROL_SHUTDOWN = 0x00000005,
            SERVICE_CONTROL_DEVICEEVENT = 0x0000000B,
            SERVICE_CONTROL_POWEREVENT = 0x0000000D
        }

        public enum PowerEventType
        {
            PBT_APMSUSPEND = 0x0004,
            PBT_APMRESUMEAUTOMATIC = 0x0012,
        }

        public enum DeviceEventType
        {
            DBT_DEVICEARRIVAL = 0x8000,
            DBT_DEVICEQUERYREMOVE = 0x8001,
            DBT_DEVICEREMOVECOMPLETE = 0x8004,
            DBT_DEVTYP_DEVICEINTERFACE = 0x0005,
            DBT_DEVTYP_HANDLE = 0x0006,
        }

       
        public const int WM_DEVICECHANGE = 0x0219;
        
        public const int WM_INPUT = 0x00ff;
        public const int WM_HOOK = 0x8000 + 1;

        [Flags]
        public enum IOCTL_BUSENUM : uint
        {
            PLUGIN_HARDWARE = 0x2A4000,
            UNPLUG_HARDWARE = 0x2A4004,
            EJECT_HARDWARE = 0x2A4008,
            REPORT_HARDWARE = 0x2A400C,

        }

        public const int IOCTL_BUSENUM_PLUGIN_HARDWARE = 0x2A4000;
        public const int IOCTL_BUSENUM_UNPLUG_HARDWARE = 0x2A4004;
        public const int IOCTL_BUSENUM_EJECT_HARDWARE = 0x2A4008;
        public const int IOCTL_BUSENUM_REPORT_HARDWARE = 0x2A400C;


        public const int DIGCF_PRESENT = 0x0002;
        public const int DIGCF_DEVICEINTERFACE = 0x0010;
        
        public delegate int ServiceControlHandlerEx(int Control, int Type, IntPtr Data, IntPtr Context);

        

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class DEV_BROADCAST_DEVICEINTERFACE_M
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;

            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            public byte[] dbcc_classguid;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            public Char[] dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_DEVICE_INTERFACE_DATA
        {
            internal int cbSize;
            internal Guid InterfaceClassGuid;
            internal int Flags;
            internal IntPtr Reserved;
        }

        
        protected const UInt32 OPEN_EXISTING = 3;
        protected const UInt32 DEVICE_SPEED = 1;
        protected const byte USB_ENDPOINT_DIRECTION_MASK = 0x80;

        protected enum POLICY_TYPE
        {
            SHORT_PACKET_TERMINATE = 1,
            AUTO_CLEAR_STALL = 2,
            PIPE_TRANSFER_TIMEOUT = 3,
            IGNORE_SHORT_PACKETS = 4,
            ALLOW_PARTIAL_READS = 5,
            AUTO_FLUSH = 6,
            RAW_IO = 7,
        }

        

        protected enum USB_DEVICE_SPEED
        {
            UsbLowSpeed = 1,
            UsbFullSpeed = 2,
            UsbHighSpeed = 3,
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct USB_CONFIGURATION_DESCRIPTOR
        {
            internal byte bLength;
            internal byte bDescriptorType;
            internal UInt16 wTotalLength;
            internal byte bNumInterfaces;
            internal byte bConfigurationValue;
            internal byte iConfiguration;
            internal byte bmAttributes;
            internal byte MaxPower;
        }

        
        protected const int DIF_PROPERTYCHANGE = 0x12;
        protected const int DICS_ENABLE = 1;
        protected const int DICS_DISABLE = 2;
        protected const int DICS_PROPCHANGE = 3;
        protected const int DICS_FLAG_GLOBAL = 1;

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_CLASSINSTALL_HEADER
        {
            internal int cbSize;
            internal int InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_PROPCHANGE_PARAMS
        {
            internal SP_CLASSINSTALL_HEADER ClassInstallHeader;
            internal int StateChange;
            internal int Scope;
            internal int HwProfile;
        }

    }
}
