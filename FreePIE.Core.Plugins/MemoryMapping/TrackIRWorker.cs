using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using FreePIE.Core.Plugins.TrackIR;

namespace FreePIE.Core.Plugins.MemoryMapping
{
    public class TrackIRWorker : SharedMemoryWorker
    {
        IntPtr headposeData;
        TrackIRDll dll;
        const short ProgramProfileId = 13302;
        const short Data = 119;

        public TrackIRWorker()
        {
            headposeData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TrackIRHeadposeData)));            
        }

        TrackIRDll SetupRealTrackIRDll(string realDllPath)
        {
            TrackIRDll dll = null;

            if (realDllPath != null)
            {
                dll = new TrackIRDll(realDllPath);
                CallStartupNPClientFunctions(dll, Data, ProgramProfileId);
            }

            return dll;
        }

        void CallStartupNPClientFunctions(TrackIRDll dll, short data, short profileId)
        {
            dll.GetSignature();
            dll.QueryVersion();
            dll.RegisterWindowHandle(Process.GetCurrentProcess().MainWindowHandle);
            dll.RequestData(data);
            dll.RegisterProgramProfileId(profileId);
            dll.StopCursor();
            dll.StartDataTransmission();
        }

        protected override void DoExecute(IEnumerable<string> arguments)
        {
            if(dll == null)
                dll = SetupRealTrackIRDll(arguments.First());

            dll.GetPosition(headposeData);

            var data = (TrackIRHeadposeData)Marshal.PtrToStructure(headposeData, typeof(TrackIRHeadposeData));

            WriteData(data);

            Thread.Sleep(1);
        }

        void WriteData(TrackIRHeadposeData headposeData)
        {
            var data = freePIEData.Read(x => x.TrackIRData);
            data.RealTrackIRData = headposeData;
            data.LastUpdatedTicks = DateTime.Now.Ticks;
            freePIEData.Write(x => x.TrackIRData, data);
        }

        public override void Dispose()
        {
            Marshal.FreeHGlobal(headposeData);
            freePIEData.Dispose();
            if (dll != null)
            {
                dll.StopDataTransmission();
                dll.StartCursor();
                dll.UnregisterWindowHandle();
                dll.Dispose();
            }
        }
    }
}