using System;
using System.Collections.Generic;
using System.IO;
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
        private bool doLog;
        const short ProgramProfileId = 13302;
        const short Data = 119;

        private void Log(string message)
        {
            if(doLog)
                using (var streamwriter = new StreamWriter("trackirworker.txt", true))
                {
                    streamwriter.WriteLine(DateTime.Now + " --|-- " + message);
                }
        }

        public TrackIRWorker()
        {
            headposeData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TrackIRHeadposeData)));            
        }

        TrackIRDll SetupRealTrackIRDll(string realDllPath, IntPtr freePieWindowHandle)
        {
            TrackIRDll dll = null;

            if (realDllPath != null)
            {
                Log("Loading trackir dll: " + realDllPath);
                dll = new TrackIRDll(realDllPath, Log);
                CallStartupNPClientFunctions(dll, freePieWindowHandle, Data, ProgramProfileId);
            } else Log("Not loading trackir dll - not found");

            return dll;
        }

        void CallStartupNPClientFunctions(TrackIRDll dll, IntPtr freePieWindowHandle, short data, short profileId)
        {
            Log("Calling dll with the following values - WindowHandle: " + freePieWindowHandle + " Data: " + data + " ProfileId: " +profileId );
            dll.GetSignature();
            dll.QueryVersion();
            dll.RegisterWindowHandle(freePieWindowHandle);
            dll.RequestData(data);
            dll.RegisterProgramProfileId(profileId);
            dll.StopCursor();
            dll.StartDataTransmission();
        }

        protected override void DoExecute(IEnumerable<string> arguments)
        {
            var args = arguments.ToList();

            doLog = args.Count == 3 && bool.Parse(args[2]);

            if(dll == null)
                dll = SetupRealTrackIRDll(args[0], new IntPtr(long.Parse(args[1])));

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