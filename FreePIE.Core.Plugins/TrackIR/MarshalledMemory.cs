using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.TrackIR
{
    public class MarshalledMemory<T> : IDisposable
    {
        public IntPtr Pointer { get; private set; }

        public T Value 
        {
            get { return (T)Marshal.PtrToStructure(Pointer, typeof (T)); }
        }

        public MarshalledMemory()
        {
            Pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (T)));
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Pointer);
        }
    }
}