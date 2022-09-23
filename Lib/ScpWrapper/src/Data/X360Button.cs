using System;

namespace JonesCorp.Data
{
    [Flags]
    public enum X360Button : ushort
    {
        None = 0,

        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,

        Start = 1 << 4,
        Back = 1 << 5,
        LS = 1 << 6,
        RS = 1 << 7,

        LB = 1 << 8,
        RB = 1 << 9,

        Guide = 1 << 10,

        A = 1 << 12,
        B = 1 << 13,
        X = 1 << 14,
        Y = 1 << 15,
    }
}