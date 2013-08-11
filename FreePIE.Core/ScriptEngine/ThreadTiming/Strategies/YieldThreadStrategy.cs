using System.Threading;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    [Timing(TimingTypes.ThreadYield)]
    public class YieldThreadStrategy : Timing
    {
        public override void Wait()
        {
            Thread.Yield();
        }
    }
}
