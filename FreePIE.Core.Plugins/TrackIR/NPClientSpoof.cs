using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.TrackIR
{
    internal class NPClientSpoof : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TrackIRHeadposeData
        {
            public ushort Status;
            public ushort FrameSignature;
            public uint IOData;

            public float Roll;
            public float Pitch;
            public float Yaw;
            public float X;
            public float Y;
            public float Z;
            public float RawX;
            public float RawY;
            public float RawZ;
            public float DeltaX;
            public float DeltaY;
            public float DeltaZ;
            public float SmoothX;
            public float SmoothY;
            public float SmoothZ;
        }

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
            TrackIRPlugin.Log("Setting up fake dll");
            DllRegistrar.InjectFakeTrackIRDll(Environment.CurrentDirectory);
            var dll = new WritableTrackIRDll(Path.Combine(Environment.CurrentDirectory, NPClientName));

            if (logFile != null)
                dll.SetupFakeNpClient(logFile);

            CallStartupNPClientFunctions(dll);

            return dll;
        }

        private readonly TrackIRDll realTrackIRDll;
        private readonly string logFile;

        private ushort lastFrame;

        private Tuple<string, short> CallStartupNPClientFunctions(TrackIRDll dll)
        {
            string signature = dll.GetSignature();
            short version = dll.QueryVersion();
            dll.RegisterWindowHandle(Process.GetCurrentProcess().MainWindowHandle);
            dll.RequestData(118);
            dll.RegisterProgramProfileId(21434);
            dll.StopCursor();
            dll.StartDataTransmission();

            return Tuple.Create(signature, version);
        }

        public NPClientSpoof(string logFile = null)
        {
            this.logFile = logFile;

            string realDllPath = DllRegistrar.GetRealTrackIRDllPath(Environment.CurrentDirectory);

            if (realDllPath != null)
            {
                TrackIRPlugin.Log("Found real trackir dll at: " + realDllPath);
                realTrackIRDll = new TrackIRDll(realDllPath + NPClientName);
                var trackIRStartupData = CallStartupNPClientFunctions(realTrackIRDll);

                TrackIRPlugin.Log("Signature: " + trackIRStartupData.Item1);
                TrackIRPlugin.Log("Version: " + trackIRStartupData.Item2);



            } else TrackIRPlugin.Log("No real trackir dll found");
        }

        int readCount = -1;
        int writeCount = -1;

        private bool ShouldLog(ref int count)
        {
            if(count++ == 2000)
            {
                count = 0;
                return true;
            }

            return false;
        }

        public bool ReadPosition(ref HeadPoseData output)
        {
            bool doLog = ShouldLog(ref readCount);

            if (realTrackIRDll == null)
            {
                TrackIRPlugin.Log("Reading attempted while no real trackir plugin found - ignoring read", doLog);
                return false;
            }

            var headpose = new InternalHeadPoseData();

            TrackIRPlugin.Log("Attempting to read data from real dll.", doLog);
            if (!ReadTrackIRData(ref headpose))
                return false;
            TrackIRPlugin.Log("Fresh data acquired", doLog);

            DecodeTrackIRIntoDegrees(headpose, doLog);

            output = new HeadPoseData() { Yaw = headpose.Yaw, Pitch = headpose.Pitch, Roll = headpose.Roll, X = headpose.X, Y = headpose.Y, Z = headpose.Z };

            return true;
        }

        private void DecodeTrackIRIntoDegrees(InternalHeadPoseData data, bool shouldLog)
        {
            TrackIRPlugin.Log("Converting TrackIR data to degrees", shouldLog);
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
            bool doLog = ShouldLog(ref writeCount);
            TrackIRPlugin.Log("Setting position using the fake trackir plugin.", doLog);
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