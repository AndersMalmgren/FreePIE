using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.TrackIR
{
    internal class TrackIRDll : IDisposable
    {
        private const string GetDataName = "NP_GetData";

        protected NativeDll dll;
        private readonly GetHeadposePosition getPosition;

        public TrackIRDll(string path)
        {
            TrackIRPlugin.Log("Loading dll at " + path);
            dll = new NativeDll(path);
            getPosition = dll.GetDelegateFromFunction<GetHeadposePosition>(GetDataName);
        }

        public int GetPosition(IntPtr data)
        {
            return getPosition(data);
        }

        private delegate int GetHeadposePosition(IntPtr data);

        public void Dispose()
        {
            dll.Dispose();
        }
    }
}
