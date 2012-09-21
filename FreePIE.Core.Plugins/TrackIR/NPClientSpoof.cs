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
        private struct InternalHeadPoseData
        {
            public float Yaw, Pitch, Roll, X, Y, Z;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SetupFakeNPClient(string logPath);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SetHeadposePosition(float yaw, float pitch, float roll, float tx, float ty, float tz);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate InternalHeadPoseData DecodeTrackirData(InternalHeadPoseData data);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate UInt32 GetHeadposePosition(ref TrackIRHeadposeData data);

        private readonly NativeDll npClient;
        private readonly SetHeadposePosition setPosition;
        private readonly GetHeadposePosition getPosition;
        private readonly DecodeTrackirData decodeHeadPose;
        private readonly SetupFakeNPClient setup;

        private const string NPClientName = @"NPClient.dll";
        private const string SetPositionName = "Freepie_SetPosition";
        private const string GetDataName = "NP_GetData";
        private const string GetSignatureName = "NP_GetSignature";
        private const string DecodeTrackirName = "Freepie_DecodeTrackirData";
        private const string SetupNPClientName = "Freepie_Setup";

        private ushort lastFrame;

        public NPClientSpoof(string logFile = null)
        {
            new DllRegistrar().InjectFakeTrackIRDll(Path.Combine(Environment.CurrentDirectory, NPClientName));
            npClient = new NativeDll(NPClientName);

            getPosition = npClient.GetDelegateFromFunction<GetHeadposePosition>(GetDataName);
            setPosition = npClient.GetDelegateFromFunction<SetHeadposePosition>(SetPositionName);
            decodeHeadPose = npClient.GetDelegateFromFunction<DecodeTrackirData>(DecodeTrackirName);
            setup = npClient.GetDelegateFromFunction<SetupFakeNPClient>(SetupNPClientName);

            if (logFile != null)
                setup(logFile);
        }

        public bool ReadPosition(ref HeadPoseData output)
        {
            var headpose = new InternalHeadPoseData();

            if (!ReadTrackIRData(ref headpose))
                return false;

            headpose = decodeHeadPose(headpose);

            output = new HeadPoseData() { Yaw = headpose.Yaw, Pitch = headpose.Pitch, Roll = headpose.Roll, X = headpose.X, Y = headpose.Y, Z = headpose.Z };

            return true;
        }

        private bool ReadTrackIRData(ref InternalHeadPoseData output)
        {
            var data = new TrackIRHeadposeData();

            getPosition(ref data);

            if (data.FrameSignature == lastFrame)
                return false;

            output = new InternalHeadPoseData() { Yaw = data.Yaw, Pitch = data.Pitch, Roll = data.Roll, X = data.X, Y = data.Y, Z = data.Z };

            lastFrame = data.FrameSignature;
            return true;
        }


        public void SetPosition(float x, float y, float z, float roll, float pitch, float yaw)
        {
            setPosition(yaw, pitch, roll, x, y, z);
        }

        public void Dispose()
        {
            npClient.Dispose();
            new DllRegistrar().EjectFakeTrackIRDll();
        }
    }
}