using System.Runtime.InteropServices;

namespace JonesCorp.Data
{
    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct SCP_XINPUT_DATA //: IEquatable<SCP_XINPUT_DATA>
    {
        

        [FieldOffset(0)]
        public int code1;

        [FieldOffset(4)]
        public int ControllerNumber;

        [FieldOffset(9)]
        public byte Code2;

        [FieldOffset(10)]
        public X360Button Buttons;

        [FieldOffset(12)]
        public byte LT;

        [FieldOffset(13)]
        public byte RT;

        [FieldOffset(14)]
        public short LX;

        [FieldOffset(16)]
        public short LY;

        [FieldOffset(18)]
        public short RX;

        [FieldOffset(20)]
        public short RY;



        

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = code1;
                hashCode = (hashCode * 397) ^ ControllerNumber;
                hashCode = (hashCode * 397) ^ Code2.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Buttons;
                hashCode = (hashCode * 397) ^ LT.GetHashCode();
                hashCode = (hashCode * 397) ^ RT.GetHashCode();
                hashCode = (hashCode * 397) ^ LX.GetHashCode();
                hashCode = (hashCode * 397) ^ LY.GetHashCode();
                hashCode = (hashCode * 397) ^ RX.GetHashCode();
                hashCode = (hashCode * 397) ^ RY.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(SCP_XINPUT_DATA other)
        {
            return Equals(other, this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (SCP_XINPUT_DATA) obj;

            return
                (other.code1 == this.code1) &&
                (other.Code2 == Code2) &&
                (other.ControllerNumber == ControllerNumber) &&
                (other.Buttons == Buttons) &&
                (other.LT == LT) &&
                (other.RT == RT) &&
                (other.LX == LX) &&
                (other.LY == LY) &&
                (other.RX == RX) &&
                (other.RY == RY);
        }

        public static bool operator ==(SCP_XINPUT_DATA a, SCP_XINPUT_DATA b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SCP_XINPUT_DATA a, SCP_XINPUT_DATA b)
        {
            return !a.Equals(b);
        }
    }
}