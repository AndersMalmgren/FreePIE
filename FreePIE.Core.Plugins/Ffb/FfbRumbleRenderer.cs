using System;

namespace FreePIE.Core.Plugins.Ffb
{
    // Rumble motors can only spin at a speed, so effects are reduced to how strong they are at a given time.
    // Conditions depend on stick position and velocity and can't be felt as rumble, they are left out
    public static class FfbRumbleRenderer
    {
        // Faster waves feel like the buzz of the small motor, slower ones like the large motor
        private const int SmallMotorMaxPeriod = 60;
        // Waves this slow are followed by the motor speed, faster ones would just average out
        private const int ModulatedMinPeriod = 150;

        public static void Render(FfbEffect effect, double elapsed, int loopCount, out double large, out double small)
        {
            large = 0;
            small = 0;

            var time = elapsed - effect.startDelay;
            if (time < 0)
                return;

            if (effect.duration != FfbEffect.Infinite)
            {
                if (effect.duration <= 0)
                    return;
                if (loopCount != FfbEffect.Infinite && time >= effect.duration * (double) Math.Max(1, loopCount))
                    return;

                time %= effect.duration;
            }

            if (effect.type == FfbEffectType.Rumble)
            {
                large = Clamp(effect.largeMotor * effect.gain);
                small = Clamp(effect.smallMotor * effect.gain);
                return;
            }

            var level = Clamp(Level(effect, time) * effect.gain);

            if (effect.type.IsPeriodic() && effect.period < SmallMotorMaxPeriod)
                small = level;
            else
                large = level;
        }

        private static double Level(FfbEffect effect, double time)
        {
            if (effect.type == FfbEffectType.Constant)
                return Math.Abs(Envelope(effect, time, Math.Abs(effect.magnitude)));

            if (effect.type == FfbEffectType.Ramp)
            {
                var ramp = effect.duration == FfbEffect.Infinite
                    ? effect.rampStart
                    : effect.rampStart + (effect.rampEnd - effect.rampStart) * time / effect.duration;

                return Math.Abs(Envelope(effect, time, Math.Abs(ramp)));
            }

            if (effect.type.IsPeriodic())
            {
                var amplitude = Envelope(effect, time, effect.magnitude);

                if (effect.period >= ModulatedMinPeriod)
                    return Math.Abs(effect.offset + amplitude * Wave(effect.type, effect.phase + time / effect.period * 360));

                return Math.Abs(effect.offset) + amplitude;
            }

            return 0;
        }

        private static double Envelope(FfbEffect effect, double time, double sustain)
        {
            if (effect.attackTime > 0 && time < effect.attackTime)
                return effect.attackLevel + (sustain - effect.attackLevel) * time / effect.attackTime;

            if (effect.fadeTime > 0 && effect.duration != FfbEffect.Infinite)
            {
                var fadeStart = effect.duration - effect.fadeTime;
                if (time > fadeStart)
                    return sustain + (effect.fadeLevel - sustain) * (time - fadeStart) / effect.fadeTime;
            }

            return sustain;
        }

        public static double Wave(FfbEffectType type, double degrees)
        {
            var cycle = degrees / 360 % 1;
            if (cycle < 0)
                cycle += 1;

            switch (type)
            {
                case FfbEffectType.Square:
                    return cycle < 0.5 ? 1 : -1;
                case FfbEffectType.Sine:
                    return Math.Sin(2 * Math.PI * cycle);
                case FfbEffectType.Triangle:
                    if (cycle < 0.25)
                        return 4 * cycle;
                    return cycle < 0.75 ? 2 - 4 * cycle : 4 * cycle - 4;
                case FfbEffectType.SawtoothUp:
                    return 2 * cycle - 1;
                case FfbEffectType.SawtoothDown:
                    return 1 - 2 * cycle;
                default:
                    return 0;
            }
        }

        private static double Clamp(double value)
        {
            return Math.Max(0, Math.Min(1, value));
        }
    }
}
