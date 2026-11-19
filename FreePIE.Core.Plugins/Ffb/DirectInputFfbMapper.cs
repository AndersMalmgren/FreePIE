using System;
using System.Linq;
using SlimDX.DirectInput;

namespace FreePIE.Core.Plugins.Ffb
{
    public static class DirectInputFfbMapper
    {
        private const int NominalMax = 10000;
        private const int DegreesToDirectInput = 100;
        private const int FullCircle = 360 * DegreesToDirectInput;
        private const int MillisecondsToMicroseconds = 1000;
        private const int NoTrigger = -1;
        private const int Infinite = -1;
        private const int LargeMotorPeriod = 80;
        private const int SmallMotorPeriod = 20;

        public static Guid? ToGuid(FfbEffectType type)
        {
            switch (type)
            {
                case FfbEffectType.Constant: return EffectGuid.ConstantForce;
                case FfbEffectType.Ramp: return EffectGuid.RampForce;
                case FfbEffectType.Square: return EffectGuid.Square;
                case FfbEffectType.Sine: return EffectGuid.Sine;
                case FfbEffectType.Triangle: return EffectGuid.Triangle;
                case FfbEffectType.SawtoothUp: return EffectGuid.SawtoothUp;
                case FfbEffectType.SawtoothDown: return EffectGuid.SawtoothDown;
                case FfbEffectType.Spring: return EffectGuid.Spring;
                case FfbEffectType.Damper: return EffectGuid.Damper;
                case FfbEffectType.Inertia: return EffectGuid.Inertia;
                case FfbEffectType.Friction: return EffectGuid.Friction;
                case FfbEffectType.Rumble: return EffectGuid.Sine;
                default: return null;
            }
        }

        // Force feedback devices have no rumble motors, rumble is played as a vibration with the feel of the strongest motor
        public static FfbEffect RumbleAsVibration(FfbEffect rumble)
        {
            var vibration = rumble.Clone();
            vibration.type = FfbEffectType.Sine;
            vibration.magnitude = Math.Max(rumble.largeMotor, rumble.smallMotor);
            vibration.period = rumble.largeMotor >= rumble.smallMotor ? LargeMotorPeriod : SmallMotorPeriod;
            vibration.offset = 0;
            // Rumble has no direction, east keeps the full strength on one axis devices
            vibration.direction = 90;

            return vibration;
        }

        public static EffectParameters ToParameters(FfbEffect effect, int[] axes)
        {
            if (effect.type == FfbEffectType.Rumble)
                effect = RumbleAsVibration(effect);

            var parameters = new EffectParameters
            {
                Duration = effect.duration == FfbEffect.Infinite ? Infinite : effect.duration * MillisecondsToMicroseconds,
                StartDelay = effect.startDelay * MillisecondsToMicroseconds,
                Gain = ToUnsigned(effect.gain),
                TriggerButton = NoTrigger
            };

            // A one axis device can't take a direction, the direction is folded into the force instead
            var projection = axes.Length == 1 ? Math.Sin(effect.direction * Math.PI / 180) : 1;

            if (effect.type.IsCondition())
            {
                parameters.Flags = EffectFlags.ObjectIds | EffectFlags.Cartesian;
                parameters.SetAxes(axes, new int[axes.Length]);
                parameters.Parameters = new ConditionSet { Conditions = ToConditions(effect, axes.Length) };
                return parameters;
            }

            if (axes.Length == 1)
            {
                parameters.Flags = EffectFlags.ObjectIds | EffectFlags.Cartesian;
                parameters.SetAxes(axes, new[] { 1 });
            }
            else
            {
                parameters.Flags = EffectFlags.ObjectIds | EffectFlags.Polar;
                parameters.SetAxes(axes, new[] { ToAngle(effect.direction), 0 });
            }

            if (effect.HasEnvelope)
                parameters.Envelope = new Envelope
                {
                    AttackLevel = ToUnsigned(effect.attackLevel),
                    AttackTime = effect.attackTime * MillisecondsToMicroseconds,
                    FadeLevel = ToUnsigned(effect.fadeLevel),
                    FadeTime = effect.fadeTime * MillisecondsToMicroseconds
                };

            if (effect.type == FfbEffectType.Constant)
                parameters.Parameters = new ConstantForce { Magnitude = ToSigned(effect.magnitude * projection) };
            else if (effect.type == FfbEffectType.Ramp)
                parameters.Parameters = new RampForce { Start = ToSigned(effect.rampStart * projection), End = ToSigned(effect.rampEnd * projection) };
            else if (effect.type.IsPeriodic())
                parameters.Parameters = new PeriodicForce
                {
                    Magnitude = ToUnsigned(effect.magnitude * Math.Abs(projection)),
                    Offset = ToSigned(effect.offset * projection),
                    Phase = ToAngle(effect.phase + (projection < 0 ? 180 : 0)),
                    Period = effect.period * MillisecondsToMicroseconds
                };

            return parameters;
        }

        private static Condition[] ToConditions(FfbEffect effect, int axisCount)
        {
            return new[] { effect.conditionX, effect.conditionY }
                .Take(axisCount)
                .Select(c => new Condition
                {
                    Offset = ToSigned(c.offset),
                    PositiveCoefficient = ToSigned(c.positiveCoefficient),
                    NegativeCoefficient = ToSigned(c.negativeCoefficient),
                    PositiveSaturation = ToUnsigned(c.positiveSaturation),
                    NegativeSaturation = ToUnsigned(c.negativeSaturation),
                    DeadBand = ToUnsigned(c.deadBand)
                })
                .ToArray();
        }

        public static int ToUnsigned(double value)
        {
            return (int) Math.Round(Math.Max(0, Math.Min(1, value)) * NominalMax);
        }

        public static int ToSigned(double value)
        {
            return (int) Math.Round(Math.Max(-1, Math.Min(1, value)) * NominalMax);
        }

        public static int ToAngle(double degrees)
        {
            var angle = (int) Math.Round(degrees * DegreesToDirectInput) % FullCircle;
            return angle < 0 ? angle + FullCircle : angle;
        }
    }
}
