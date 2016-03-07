using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.VJoy
{
    [StructLayout(LayoutKind.Explicit)]
    public struct EffectReport
    {
        //[FieldOffset(0)]
        //private byte DevIDXAndPacketType;
        [FieldOffset(1)]
        public byte BlockIndex;
        [FieldOffset(2)]
        public EffectType EffectType;
        [FieldOffset(3)]
        public short Duration;
        [FieldOffset(5)]
        public short TriggerRepeatInterval;
        [FieldOffset(7)]
        public short SamplePeriod;
        [FieldOffset(9)]
        public byte Gain;
        [FieldOffset(10)]
        public byte TriggerBtn; //button?
        [FieldOffset(11)]
        private byte PolarByte;
        [FieldOffset(12)]
        public byte Direction;
        [FieldOffset(12)]
        public byte DirectionX;
        [FieldOffset(13)]
        public byte DirectionY;

        public bool Polar { get { return PolarByte == 0x04; } }
        public int AngleInDegrees
        {
            get
            {
                if (!Polar)
                    throw new System.Exception("This EffectReport is not in polar coordinates");
                return Direction * 360 / 255;
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\tBlockIndex: ");
            sb.AppendLine(BlockIndex.ToString());
            sb.Append("\tEffectType: ");
            sb.AppendLine(EffectType.ToString());
            sb.Append("\tDuration: ");
            sb.AppendLine(Duration.ToString());
            sb.Append("\tTriggerRepeatInterval: ");
            sb.AppendLine(TriggerRepeatInterval.ToString());
            sb.Append("\tSamplePeriod: ");
            sb.AppendLine(SamplePeriod.ToString());
            sb.Append("\tGain: ");
            sb.AppendLine(Gain.ToString());
            sb.Append("\tTriggerBtn (?): ");
            sb.AppendLine(TriggerBtn.ToString());
            sb.Append("\tPolar: ");
            sb.AppendLine(Polar.ToString());
            if (Polar)
            {
                sb.Append("\tAngle: ");
                sb.AppendLine(AngleInDegrees.ToString());
            } else
            {
                sb.AppendFormat("\tX: {0:5}, Y:{1:5}\n", DirectionX, DirectionY);
            }
            return sb.ToString();
        }
    }
}
