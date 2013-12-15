using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using FreePIE.Core.Plugins.MemoryMapping;

namespace FreePIE.Core.Plugins.TrackIR
{
    internal class NPClientSpoof : IDisposable
    {
        private readonly bool doLog;

        internal class InternalHeadPoseData
        {
            public float Yaw, Pitch, Roll, X, Y, Z;
        }

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CopyMemory(IntPtr pDest, IntPtr pSrc, int length);

        internal const string NPClientName = @"NPClient.dll";
        private readonly MappedMemory<DisconnectedFreepieData> freepieData;
        private bool hasInjectedDll;
        private WorkerProcess<TrackIRWorker> trackIRWorker;
        private readonly Mutex freePieTrackIRMutex = new Mutex(false, "Freepie.TrackIRMutex");
        private const int WorkerProcessTimeout = 20;

        void SetupFakeTrackIR()
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, NPClientName)))
            {
                if(doLog)
                    InitDllLogging();

                DllRegistrar.InjectFakeTrackIRDll(Environment.CurrentDirectory + @"\");
                hasInjectedDll = true;
            } else Debug.WriteLine("No fake trackir dll found in current directory - obviously not spoofing.");
        }

        private unsafe void InitDllLogging()
        {
            var fakeTrackirData = new FreePieTrackIRHeadposeData();

            using (var logpath = new MarshalledString(Path.Combine(Environment.CurrentDirectory, "NPClient.log")))
                CopyMemory(new IntPtr(fakeTrackirData.LogPath), logpath.Pointer, logpath.Length);

            freepieData.Write(x => x.TrackIRData.FakeTrackIRData, fakeTrackirData);
        }

        private WorkerProcess<TrackIRWorker> SetupRealTrackIRDll()
        {
            string realDllPath = DllRegistrar.GetRealTrackIRDllPath(Environment.CurrentDirectory);

            if (realDllPath == null)
                return new WorkerProcess<TrackIRWorker>();

            freepieData.Write(x => x.TrackIRData, new TrackIRData { LastUpdatedTicks = DateTime.Now.Ticks });
            var worker = new WorkerProcess<TrackIRWorker>(Path.Combine(realDllPath, NPClientName).Quote() + " " + Process.GetCurrentProcess().MainWindowHandle.ToInt64() + " " + doLog);
            worker.Start();
            return worker;
        }


        public NPClientSpoof(bool doLog)
        {
            this.doLog = doLog;
            freepieData = new MappedMemory<DisconnectedFreepieData>(DisconnectedFreepieData.SharedMemoryName);
        }

        public bool ReadPosition(ref HeadPoseData output)
        {
            var headpose = new InternalHeadPoseData();

            if (!ReadTrackIRData(ref headpose))
                return false;

            DecodeTrackIRIntoDegrees(headpose);

            output = new HeadPoseData { Yaw = headpose.Yaw, Pitch = headpose.Pitch, Roll = headpose.Roll, X = headpose.X, Y = headpose.Y, Z = headpose.Z };

            return true;
        }

        private void DecodeTrackIRIntoDegrees(InternalHeadPoseData data)
        {
            data.Yaw = -(data.Yaw * 180.0f) / 16384.0f;
            data.Pitch = -(data.Pitch * 180.0f) / 16384.0f;
            data.Roll = -(data.Roll * 180.0f) / 16384.0f;

            data.X = -data.X / 64.0f;
            data.Y = data.Y / 64.0f;
            data.Z = data.Z / 64.0f;
        }

        private ushort lastFrame;

        private bool ReadTrackIRData(ref InternalHeadPoseData output)
        {
            if (trackIRWorker == null)
                trackIRWorker = SetupRealTrackIRDll();

            var trackirData = freepieData.Read(x => x.TrackIRData);

            if (DateTime.Now - new DateTime(trackirData.LastUpdatedTicks) > TimeSpan.FromSeconds(WorkerProcessTimeout))
            {
                trackIRWorker.Dispose();
                throw new Exception("Lost contact with worker process.");
            }

            var data = trackirData.RealTrackIRData;

            if (data.FrameSignature == lastFrame || data.FrameSignature == 0)
                return false;

            output = new InternalHeadPoseData { Yaw = data.Yaw, Pitch = data.Pitch, Roll = data.Roll, X = data.X, Y = data.Y, Z = data.Z };

            lastFrame = data.FrameSignature;

            return true;
        }

        public void SetPosition(float x, float y, float z, float roll, float pitch, float yaw)
        {
            if(!hasInjectedDll)
                SetupFakeTrackIR();

	        if(freePieTrackIRMutex.WaitOne(10))
	        {
                var trackIr = freepieData.Read(f => f.TrackIRData);

	            trackIr.FakeTrackIRData.FrameNumber++;

	            trackIr.FakeTrackIRData.Yaw = yaw;
	            trackIr.FakeTrackIRData.Pitch = pitch;
	            trackIr.FakeTrackIRData.Roll = roll;
	            trackIr.FakeTrackIRData.X = x;
	            trackIr.FakeTrackIRData.Y = y;
	            trackIr.FakeTrackIRData.Z = z;

                freepieData.Write(f => f.TrackIRData, trackIr);

                freePieTrackIRMutex.ReleaseMutex();
	        }
        }

        public void Dispose()
        {
            if (hasInjectedDll)
                DllRegistrar.EjectFakeTrackIRDll();

            freePieTrackIRMutex.Dispose();

            if(trackIRWorker != null)
                trackIRWorker.Dispose();
        }
    }
}