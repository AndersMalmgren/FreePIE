using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Ffb
{
    [Documentation("Parameters of a condition effect (spring, damper, inertia, friction) for one axis")]
    public class FfbCondition
    {
        [Documentation("Center point of the condition, -1 to 1")]
        public double offset { get; set; }
        [Documentation("Strength on the positive side of the center, -1 to 1")]
        public double positiveCoefficient { get; set; }
        [Documentation("Strength on the negative side of the center, -1 to 1")]
        public double negativeCoefficient { get; set; }
        [Documentation("Maximum force on the positive side, 0 to 1")]
        public double positiveSaturation { get; set; }
        [Documentation("Maximum force on the negative side, 0 to 1")]
        public double negativeSaturation { get; set; }
        [Documentation("Region around the center where the condition is inactive, 0 to 1")]
        public double deadBand { get; set; }

        public FfbCondition()
        {
            positiveSaturation = 1;
            negativeSaturation = 1;
        }

        public FfbCondition Clone()
        {
            return (FfbCondition) MemberwiseClone();
        }

        public bool SameAs(FfbCondition other)
        {
            return offset == other.offset
                && positiveCoefficient == other.positiveCoefficient
                && negativeCoefficient == other.negativeCoefficient
                && positiveSaturation == other.positiveSaturation
                && negativeSaturation == other.negativeSaturation
                && deadBand == other.deadBand;
        }
    }
}
