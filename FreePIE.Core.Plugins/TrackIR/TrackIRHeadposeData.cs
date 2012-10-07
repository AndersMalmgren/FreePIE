using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.TrackIR
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FreePieTrackIRHeadposeData
    {
        public float Yaw, Pitch, Roll, X, Y, Z;
        public UInt32 FrameNumber;
        public fixed byte LogPath[200];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrackIRData
    {
        public FreePieTrackIRHeadposeData FakeTrackIRData;
        public TrackIRHeadposeData RealTrackIRData;
        public long LastUpdatedTicks;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrackIRHeadposeData
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
}