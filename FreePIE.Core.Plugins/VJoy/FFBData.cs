using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.VJoy
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FfbData
    {
        public int DataSize;
        public int Command;
        public IntPtr PtrToData;
        public byte[] Data
        {
            get
            {
                if (DataSize < 10)
                    throw new Exception(string.Format("DataSize incorrect, {0}", DataSize));
                byte[] outBuffer = new byte[DataSize];
                Marshal.Copy(PtrToData, outBuffer, 0, DataSize - 8);//last 8 bytes are not interesting? (haven't seen them in use anywhere anyway)
                return outBuffer;
            }
        }
    }
}
