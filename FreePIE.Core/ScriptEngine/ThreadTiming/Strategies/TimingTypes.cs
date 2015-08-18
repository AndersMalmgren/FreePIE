using FreePIE.Core.Contracts;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    [GlobalEnum]
    public enum TimingTypes
    {
        SystemTimer = 1,
        ThreadYield = 2,
        HighresSystemTimer = 3,
        MicroTimer = 4
    }
}
