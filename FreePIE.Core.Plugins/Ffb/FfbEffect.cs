using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Ffb
{
    [Documentation("A force feedback effect. Only the parameters used by its type matter")]
    public class FfbEffect
    {
        public const int Infinite = -1;

        [Documentation("Type of the effect")]
        public FfbEffectType type { get; set; }
        [Documentation("Duration in milliseconds, -1 plays until stopped")]
        public int duration { get; set; }
        [Documentation("Delay in milliseconds before the effect starts")]
        public int startDelay { get; set; }
        [Documentation("Strength of the effect, 0 to 1")]
        public double gain { get; set; }
        [Documentation("Direction the force comes from in degrees, 0 is north and 90 is east")]
        public double direction { get; set; }

        [Documentation("Force of a constant effect -1 to 1, amplitude of a periodic effect 0 to 1")]
        public double magnitude { get; set; }
        [Documentation("Force at the start of a ramp, -1 to 1")]
        public double rampStart { get; set; }
        [Documentation("Force at the end of a ramp, -1 to 1")]
        public double rampEnd { get; set; }
        [Documentation("Offset of a periodic wave, -1 to 1")]
        public double offset { get; set; }
        [Documentation("Phase of a periodic wave in degrees")]
        public double phase { get; set; }
        [Documentation("Period of a periodic wave in milliseconds")]
        public int period { get; set; }

        [Documentation("Level at the start of the effect, 0 to 1")]
        public double attackLevel { get; set; }
        [Documentation("Time in milliseconds to go from attackLevel to full strength")]
        public int attackTime { get; set; }
        [Documentation("Level at the end of the effect, 0 to 1")]
        public double fadeLevel { get; set; }
        [Documentation("Time in milliseconds to go from full strength to fadeLevel")]
        public int fadeTime { get; set; }

        [Documentation("Speed of the large, low frequency rumble motor, 0 to 1")]
        public double largeMotor { get; set; }
        [Documentation("Speed of the small, high frequency rumble motor, 0 to 1")]
        public double smallMotor { get; set; }

        [Documentation("Condition parameters for the first axis")]
        public FfbCondition conditionX { get; private set; }
        [Documentation("Condition parameters for the second axis")]
        public FfbCondition conditionY { get; private set; }

        [Documentation("True when the effect has been started and not stopped, set it to start or stop the effect")]
        public bool playing { get; set; }

        public FfbEffect()
        {
            duration = Infinite;
            gain = 1;
            period = 100;
            conditionX = new FfbCondition();
            conditionY = new FfbCondition();
        }

        public bool HasEnvelope
        {
            get { return attackTime > 0 || fadeTime > 0; }
        }

        public FfbEffect Clone()
        {
            var clone = (FfbEffect) MemberwiseClone();
            clone.conditionX = conditionX.Clone();
            clone.conditionY = conditionY.Clone();

            return clone;
        }

        public bool SameParametersAs(FfbEffect other)
        {
            return type == other.type
                && duration == other.duration
                && startDelay == other.startDelay
                && gain == other.gain
                && direction == other.direction
                && magnitude == other.magnitude
                && rampStart == other.rampStart
                && rampEnd == other.rampEnd
                && offset == other.offset
                && phase == other.phase
                && period == other.period
                && attackLevel == other.attackLevel
                && attackTime == other.attackTime
                && fadeLevel == other.fadeLevel
                && fadeTime == other.fadeTime
                && largeMotor == other.largeMotor
                && smallMotor == other.smallMotor
                && conditionX.SameAs(other.conditionX)
                && conditionY.SameAs(other.conditionY);
        }
    }
}
