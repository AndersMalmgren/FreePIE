using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.TrackIR
{
    public class MarshalledString : IDisposable
    {
        private readonly int length;
        public IntPtr Pointer { get; private set; }

        public string Value
        {
            get { return Marshal.PtrToStringAnsi(Pointer, length); }
        }

        public MarshalledString(int length)
        {
            this.length = length;
            Pointer = Marshal.AllocHGlobal(length);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Pointer);
        }
    }
}