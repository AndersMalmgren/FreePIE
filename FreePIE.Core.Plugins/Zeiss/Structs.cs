using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Zeiss
{
    public struct Vec3
    {
        public float x, y, z;
    }

    public struct Quat
    {
        public float w;
        public Vec3 v;
    }

    public struct Frame
    {
        public Vec3 Acc, Gyr, Mag;
        public Quat Rot;
        public long FrameNumber;
    }

    public struct Euler
    {
        public float Yaw, Pitch, Roll;
    }
}
