using System;
using System.Diagnostics;
using System.Threading;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    [Timing(TimingTypes.ThreadYield)]
    public class YieldThreadStrategy : Timing
    {
        private readonly MicroStopWatch watch;
        private long lastWaitEnded;

        public YieldThreadStrategy(bool unitInMicroSeconds = false)
        {
            watch = new MicroStopWatch(unitInMicroSeconds);
        }

        public override void Wait()
        {
            EnsureWatchStarted();

            while ((watch.ElapsedUnits - lastWaitEnded) < ThreadExecutionInterval)
                Thread.Yield();

            lastWaitEnded = watch.ElapsedUnits;
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

        private class MicroStopWatch : Stopwatch
        {
            private readonly bool unitInMicroSeconds;
            private static readonly double MicroSecPerTick = +1000000D/Frequency;

            public MicroStopWatch(bool unitInMicroSeconds)
            {
                this.unitInMicroSeconds = unitInMicroSeconds;
                if (unitInMicroSeconds && !IsHighResolution)
                    throw new Exception("On this system the high-resolution performance counter is not available");
            }

            public long ElapsedUnits
            {
                get
                {
                    if (!unitInMicroSeconds) return ElapsedMilliseconds;

                    return (long) (ElapsedTicks*MicroSecPerTick);
                }
            }
        }
    }
}
