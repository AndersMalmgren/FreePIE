using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScpDotNet
{
    [StructLayout(LayoutKind.Sequential,Pack = 1)]
    public struct SCP_INPUT_REPORT
    {
        public DsPadId Pad;
        public DsState State;
        public byte Battery;
        public byte Connection;
        
        public UInt32 PacketNumber;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public byte[] RawData;
        
        public byte Model;

        [MarshalAs(UnmanagedType.ByValArray,SizeConst = 6)]
        public byte[] Address;

    }
}
