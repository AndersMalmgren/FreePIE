using System.Threading;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    [Timing(TimingTypes.SystemTimer, Default = true)]
    public class SystemTimerStrategy : Timing
    {
        public override void Wait()
        {
            Thread.Sleep(1);
        }
    }
}
