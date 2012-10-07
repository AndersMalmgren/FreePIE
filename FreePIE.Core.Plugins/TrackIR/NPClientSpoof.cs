using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace FreePIE.Core.Plugins.TrackIR
{
    internal class NPClientSpoof : IDisposable
    {
        internal class InternalHeadPoseData
        {
            public float Yaw, Pitch, Roll, X, Y, Z;
        }

        private const string NPClientName = @"NPClient.dll";
        private WritableTrackIRDll freepieDll;

        private WritableTrackIRDll FreepieDll
        {
            get 
            { 
                return freepieDll ?? (freepieDll = SetupFakeTrackIR());
            }
        }

        WritableTrackIRDll SetupFakeTrackIR()
        {
            DllRegistrar.InjectFakeTrackIRDll(Environment.CurrentDirectory);
            var dll = new WritableTrackIRDll(Path.Combine(Environment.CurrentDirectory, NPClientName));

            if (logFile != null)
                dll.SetupFakeNpClient(logFile);

            return dll;
        }

        private TrackIRDll realTrackIRDll;

        private TrackIRDll RealTrackIRDll
        {
            get { return realTrackIRDll ?? (realTrackIRDll = SetupRealTrackIRDll());
}
        }

        private TrackIRDll SetupRealTrackIRDll()
        {
            string realDllPath = DllRegistrar.GetRealTrackIRDllPath(Environment.CurrentDirectory);

            TrackIRDll dll = null;

            if (realDllPath != null)
            {
                dll = new TrackIRDll(realDllPath + NPClientName);
                CallStartupNPClientFunctions(dll, Data, ProgramProfileId);
            }

            return dll;
        }

        private readonly string logFile;
        private ushort lastFrame;

        private void CallStartupNPClientFunctions(TrackIRDll dll, short data, short profileId)
        {
            dll.GetSignature();
            dll.QueryVersion();
            dll.RegisterWindowHandle(Process.GetCurrentProcess().MainWindowHandle);
            dll.RequestData(data);
            dll.RegisterProgramProfileId(profileId);
            dll.StopCursor();
            dll.StartDataTransmission();
        }

        private const short ProgramProfileId = 13302;
        private const short Data = 119;

        public NPClientSpoof(string logFile = null)
        {
            this.logFile = logFile;
        }

        public bool ReadPosition(ref HeadPoseData output)
        {
            if (RealTrackIRDll == null)
                return false;

            var headpose = new InternalHeadPoseData();

            if (!ReadTrackIRData(ref headpose))
                return false;

            DecodeTrackIRIntoDegrees(headpose);

            output = new HeadPoseData() { Yaw = headpose.Yaw, Pitch = headpose.Pitch, Roll = headpose.Roll, X = headpose.X, Y = headpose.Y, Z = headpose.Z };

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

        private bool ReadTrackIRData(ref InternalHeadPoseData output)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (TrackIRHeadposeData)));

            realTrackIRDll.GetPosition(ptr);

            TrackIRHeadposeData data = (TrackIRHeadposeData)Marshal.PtrToStructure(ptr, typeof (TrackIRHeadposeData));

            Marshal.FreeHGlobal(ptr);

            if (data.FrameSignature == lastFrame || data.FrameSignature == 0)
                return false;

            output = new InternalHeadPoseData() { Yaw = data.Yaw, Pitch = data.Pitch, Roll = data.Roll, X = data.X, Y = data.Y, Z = data.Z };

            lastFrame = data.FrameSignature;
            return true;
        }

        public void SetPosition(float x, float y, float z, float roll, float pitch, float yaw)
        {
            FreepieDll.SetPosition(yaw, pitch, roll, x, y, z);
        }

        public void Dispose()
        {
            if (freepieDll != null)
            {
                DllRegistrar.EjectFakeTrackIRDll();
                freepieDll.Dispose();
            }

            if (realTrackIRDll != null)
            {
                realTrackIRDll.StopDataTransmission();
                realTrackIRDll.StartCursor();
                realTrackIRDll.UnregisterWindowHandle();
                realTrackIRDll.Dispose();
            }
        }
    }
}