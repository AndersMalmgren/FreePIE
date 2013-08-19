using System.Diagnostics;
using System.Threading;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    [Timing(TimingTypes.ThreadYield)]
    public class YieldThreadStrategy : Timing
    {
        private readonly Stopwatch watch;
        private long lastWaitEnded;

        public YieldThreadStrategy()
        {
            watch = new Stopwatch();
        }

        public override void Wait()
        {
            EnsureWatchStarted();

            while ((watch.ElapsedMilliseconds - lastWaitEnded) < ThreadExecutionInterval)
                Thread.Yield();

            lastWaitEnded = watch.ElapsedMilliseconds;
        }

        private void EnsureWatchStarted()
        {
            if (watch.IsRunning)
                return;

            lastWaitEnded = 0;
            watch.Start();
        }

        public override void Dispose()
        {
            watch.Stop();
        }
    }
}
