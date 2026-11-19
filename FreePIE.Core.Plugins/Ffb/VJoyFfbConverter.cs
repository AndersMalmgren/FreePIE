using vJoyInterfaceWrap;

namespace FreePIE.Core.Plugins.Ffb
{
    // Value ranges are those documented for the vJoy 2.1.8 FFB API
    public static class VJoyFfbConverter
    {
        private const double MaxForce = 10000.0;
        private const double MaxByte = 255.0;
        private const ushort InfiniteDuration = 0xFFFF;
        private const byte InfiniteLoopCount = 0xFF;

        public static FfbEffectType ToEffectType(FFBEType type)
        {
            switch (type)
            {
                case FFBEType.ET_CONST: return FfbEffectType.Constant;
                case FFBEType.ET_RAMP: return FfbEffectType.Ramp;
                case FFBEType.ET_SQR: return FfbEffectType.Square;
                case FFBEType.ET_SINE: return FfbEffectType.Sine;
                case FFBEType.ET_TRNGL: return FfbEffectType.Triangle;
                case FFBEType.ET_STUP: return FfbEffectType.SawtoothUp;
                case FFBEType.ET_STDN: return FfbEffectType.SawtoothDown;
                case FFBEType.ET_SPRNG: return FfbEffectType.Spring;
                case FFBEType.ET_DMPR: return FfbEffectType.Damper;
                case FFBEType.ET_INRT: return FfbEffectType.Inertia;
                case FFBEType.ET_FRCTN: return FfbEffectType.Friction;
                default: return FfbEffectType.None;
            }
        }

        public static void Apply(FfbEffect effect, vJoy.FFB_EFF_REPORT report)
        {
            effect.type = ToEffectType(report.EffectType);
            effect.duration = report.Duration == InfiniteDuration ? FfbEffect.Infinite : report.Duration;
            effect.gain = report.Gain / MaxByte;
            // vJoy puts the first direction in the same byte whether the effect is polar or not
            effect.direction = (report.Polar ? report.Direction : report.DirX) * 360 / MaxByte;
        }

        public static void Apply(FfbEffect effect, vJoy.FFB_EFF_CONSTANT constant)
        {
            effect.magnitude = constant.Magnitude / MaxForce;
        }

        public static void Apply(FfbEffect effect, vJoy.FFB_EFF_RAMP ramp)
        {
            effect.rampStart = ramp.Start / MaxForce;
            effect.rampEnd = ramp.End / MaxForce;
        }

        public static void Apply(FfbEffect effect, vJoy.FFB_EFF_PERIOD periodic)
        {
            effect.magnitude = periodic.Magnitude / MaxForce;
            effect.offset = periodic.Offset / MaxForce;
            effect.phase = periodic.Phase / 100.0;
            effect.period = (int) periodic.Period;
        }

        public static void Apply(FfbEffect effect, vJoy.FFB_EFF_ENVLP envelope)
        {
            effect.attackLevel = envelope.AttackLevel / MaxForce;
            effect.attackTime = (int) envelope.AttackTime;
            effect.fadeLevel = envelope.FadeLevel / MaxForce;
            effect.fadeTime = (int) envelope.FadeTime;
        }

        public static void Apply(FfbEffect effect, vJoy.FFB_EFF_COND condition)
        {
            var target = condition.isY ? effect.conditionY : effect.conditionX;

            target.offset = condition.CenterPointOffset / MaxForce;
            target.positiveCoefficient = condition.PosCoeff / MaxForce;
            target.negativeCoefficient = condition.NegCoeff / MaxForce;
            target.positiveSaturation = condition.PosSatur / MaxForce;
            target.negativeSaturation = condition.NegSatur / MaxForce;
            target.deadBand = condition.DeadBand / MaxForce;
        }

        public static int ToLoopCount(byte loopCount)
        {
            return loopCount == InfiniteLoopCount ? FfbEffect.Infinite : loopCount;
        }

        public static double ToGain(byte gain)
        {
            return gain / MaxByte;
        }

        public static FfbControl ToControl(FFB_CTRL control)
        {
            switch (control)
            {
                case FFB_CTRL.CTRL_ENACT: return FfbControl.EnableActuators;
                case FFB_CTRL.CTRL_DISACT: return FfbControl.DisableActuators;
                case FFB_CTRL.CTRL_STOPALL: return FfbControl.StopAll;
                case FFB_CTRL.CTRL_DEVPAUSE: return FfbControl.Pause;
                case FFB_CTRL.CTRL_DEVCONT: return FfbControl.Continue;
                default: return FfbControl.Reset;
            }
        }
    }
}
