using System;
using System.Runtime.InteropServices;

namespace vJoyFFBWrapper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFBData
    {
        public int DataSize;
        public int Command;
        public IntPtr PtrToData;
        public byte[] Data
        {
            get
            {
                if (DataSize < 10)
                    throw new Exception($"DataSize incorrect, {DataSize}");
                byte[] outBuffer = new byte[DataSize];
                Marshal.Copy(PtrToData, outBuffer, 0, DataSize - 8);//last 8 bytes are not interesting? (haven't seen them in use anywhere anyway)
                return outBuffer;
            }
        }
    }
}
