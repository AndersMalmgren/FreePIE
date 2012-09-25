using System;
using System.Collections.Generic;
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct InternalHeadPoseData
        {
            public float Yaw, Pitch, Roll, X, Y, Z;
        }

        private const string NPClientName = @"NPClient.dll";

        private readonly WritableTrackIRDll freepieDll;
        private readonly TrackIRDll realTrackIRDll;

        private ushort lastFrame;

        public NPClientSpoof(string logFile = null)
        {
            string realDllPath = DllRegistrar.InjectFakeTrackIRDll(Environment.CurrentDirectory);
            freepieDll = new WritableTrackIRDll(NPClientName);

            if(realDllPath != null)
                realTrackIRDll = new TrackIRDll(realDllPath + NPClientName);

            if (logFile != null)
                freepieDll.SetupFakeNpClient(logFile);
        }

        public bool ReadPosition(ref HeadPoseData output)
        {
            if(realTrackIRDll == null)
                throw new InvalidOperationException("Reading not supported - real trackir dll not loaded.");

            var headpose = new InternalHeadPoseData();

            if (!ReadTrackIRData(ref headpose))
                return false;

            headpose = freepieDll.DecodeTrackir(headpose);

            output = new HeadPoseData() { Yaw = headpose.Yaw, Pitch = headpose.Pitch, Roll = headpose.Roll, X = headpose.X, Y = headpose.Y, Z = headpose.Z };

            return true;
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

            System.Diagnostics.Debug.WriteLine("Yaw: " + output.Yaw + "frame: " + data.FrameSignature);

            lastFrame = data.FrameSignature;
            return true;
        }


        public void SetPosition(float x, float y, float z, float roll, float pitch, float yaw)
        {
            freepieDll.SetPosition(yaw, pitch, roll, x, y, z);
        }

        public void Dispose()
        {
            freepieDll.Dispose();

            if(realTrackIRDll != null)
                realTrackIRDll.Dispose();

            DllRegistrar.EjectFakeTrackIRDll();
        }
    }
}