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

        public TrackIRDll(string path)
        {
            dll = new NativeDll(path);
            getPosition = dll.GetDelegateFromFunction<GetHeadposePosition>(GetDataName);

        }

        public int GetPosition(IntPtr data)
        {
            return getPosition(data);
        }

        private delegate int GetHeadposePosition(IntPtr data);
        private readonly GetHeadposePosition getPosition;

        public void Dispose()
        {
            dll.Dispose();
        }
    }
}
