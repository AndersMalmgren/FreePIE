using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.MemoryMapping
{
    public class MarshalledString : IDisposable
    {
        public IntPtr Pointer { get; private set; }

        public string Value
        {
            get { return Marshal.PtrToStringAnsi(Pointer, Length); }
        }

        public int Length { get; private set; }

        public MarshalledString(int length)
        {
            Length = length;
            Pointer = Marshal.AllocHGlobal(length);
        }

        public MarshalledString(string value)
        {
            Length = value.Length + 1;

            Pointer = Marshal.StringToHGlobalAnsi(value);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Pointer);
        }
    }
}