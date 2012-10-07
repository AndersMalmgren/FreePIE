using System.Runtime.InteropServices;
using FreePIE.Core.Plugins.TrackIR;

namespace FreePIE.Core.Plugins.MemoryMapping
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DisconnectedFreepieData
    {
        public readonly TrackIRData TrackIRData;
    }
}
