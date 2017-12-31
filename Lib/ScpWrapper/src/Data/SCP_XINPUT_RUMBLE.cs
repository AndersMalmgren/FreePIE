using System.Runtime.InteropServices;

namespace JonesCorp.Data
{
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct SCP_XINPUT_RUMBLE
    {
        public int Large;

        public int Small;

    }
}