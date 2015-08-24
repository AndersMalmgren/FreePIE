namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    [Timing(TimingTypes.ThreadYieldMicroSeconds)]
    public class ThreadYieldMicroSecondsStrategy : YieldThreadStrategy
    {
        public ThreadYieldMicroSecondsStrategy() : base(true)
        {
        }
    }
}