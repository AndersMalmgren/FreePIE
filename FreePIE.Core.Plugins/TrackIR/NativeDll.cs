using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.TrackIR
{
    public class NativeDll : IDisposable
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllname);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procname);

        readonly string dll;
        private readonly IntPtr library;

        public NativeDll(string dll)
        {
            this.dll = dll;
            library = LoadLibrary(dll);

            if (library == IntPtr.Zero)
                throw new Exception("Cannot load library: " + library);
        }

        public void Dispose()
        {
            if (library != IntPtr.Zero)
                FreeLibrary(library);
        }

        public T GetDelegateFromFunction<T>(string procName)
        {
            IntPtr ptr = GetProcAddress(library, procName);

            if (ptr == IntPtr.Zero)
                throw new Win32Exception("Cannot load function: " + procName);

            object obj = Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
            return (T)obj;
        }
    }
}
