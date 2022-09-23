using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace JonesCorp
{
    public partial class WinUsbDevice
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        public static string GetLastErrorDescription(uint lastErrorID)
        {
            string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            return errorMessage;
        }


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, UInt32 hTemplateFile);


        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern int SetupDiCreateDeviceInfoList(ref System.Guid ClassGuid, int hwndParent);

        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern int SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref System.Guid InterfaceClassGuid, int MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr SetupDiGetClassDevs(ref System.Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, int Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, IntPtr DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, ref SP_DEVICE_INTERFACE_DATA DeviceInfoData);

        

        

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr RegisterServiceCtrlHandlerEx(String ServiceName, ServiceControlHandlerEx Callback, IntPtr Context);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr DeviceHandle, IOCTL_BUSENUM IoControlCode, byte[] InBuffer, int InBufferSize, byte[] OutBuffer, int OutBufferSize, ref int BytesReturned, IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool CloseHandle(IntPtr Handle);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern int CM_Get_Device_ID(int dnDevInst, IntPtr Buffer, int BufferLen, int ulFlags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool SetupDiOpenDeviceInfo(IntPtr DeviceInfoSet, String DeviceInstanceId, IntPtr hwndParent, int Flags, ref SP_DEVICE_INTERFACE_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool SetupDiChangeState(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, ref SP_PROPCHANGE_PARAMS ClassInstallParams, int ClassInstallParamsSize);

    }
}
