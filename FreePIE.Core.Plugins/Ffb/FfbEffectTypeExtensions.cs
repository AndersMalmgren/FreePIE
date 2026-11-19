namespace FreePIE.Core.Plugins.Ffb
{
    public static class FfbEffectTypeExtensions
    {
        public static bool IsCondition(this FfbEffectType type)
        {
            return type == FfbEffectType.Spring || type == FfbEffectType.Damper || type == FfbEffectType.Inertia || type == FfbEffectType.Friction;
        }

        public static bool IsPeriodic(this FfbEffectType type)
        {
            return type == FfbEffectType.Square || type == FfbEffectType.Sine || type == FfbEffectType.Triangle || type == FfbEffectType.SawtoothUp || type == FfbEffectType.SawtoothDown;
        }
    }
}
