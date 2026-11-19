using FreePIE.Core.Contracts;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    [GlobalEnum]
    [Documentation("How the script thread waits between iterations, set with system.setThreadTiming")]
    public enum TimingTypes
    {
        [Documentation("Sleeps with the system timer, the default. Low CPU usage but the resolution is limited by the system timer")]
        SystemTimer = 1,
        [Documentation("Yields the thread until the interval in milliseconds has passed. Accurate but uses more CPU")]
        ThreadYield = 2,
        [Documentation("Sleeps with the system timer set to 1 ms resolution")]
        HighresSystemTimer = 3,
        [Documentation("Like ThreadYield but threadExecutionInterval is in microseconds")]
        ThreadYieldMicroSeconds = 4,
        [Documentation("Sub-millisecond sleep using Windows high-resolution waitable timers without altering global timer resolution\nor spinning CPU. threadExecutionInterval is in milliseconds")]
        HighResWaitableTimer = 5
    }
}
