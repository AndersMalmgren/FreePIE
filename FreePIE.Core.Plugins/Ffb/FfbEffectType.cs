using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Ffb
{
    [GlobalEnum]
    [Documentation("Force feedback effect types")]
    public enum FfbEffectType
    {
        [Documentation("No effect")]
        None,
        [Documentation("Constant force, uses magnitude")]
        Constant,
        [Documentation("Force that changes linearly from rampStart to rampEnd over the duration")]
        Ramp,
        [Documentation("Periodic square wave, uses magnitude, offset, phase and period")]
        Square,
        [Documentation("Periodic sine wave, uses magnitude, offset, phase and period")]
        Sine,
        [Documentation("Periodic triangle wave, uses magnitude, offset, phase and period")]
        Triangle,
        [Documentation("Periodic rising sawtooth wave, uses magnitude, offset, phase and period")]
        SawtoothUp,
        [Documentation("Periodic falling sawtooth wave, uses magnitude, offset, phase and period")]
        SawtoothDown,
        [Documentation("Condition that pulls towards the center, uses the conditions")]
        Spring,
        [Documentation("Condition that resists velocity, uses the conditions")]
        Damper,
        [Documentation("Condition that resists acceleration, uses the conditions")]
        Inertia,
        [Documentation("Condition that resists movement, uses the conditions")]
        Friction,
        [Documentation("Gamepad rumble, uses largeMotor and smallMotor")]
        Rumble
    }
}
