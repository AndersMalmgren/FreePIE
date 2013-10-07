using System.Runtime.InteropServices;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    [Timing(TimingTypes.HighresSystemTimer)]
    public class BeginPeriodSystemTimerStrategy : SystemTimerStrategy
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern uint BeginPeriod(uint milliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint EndPeriod(uint milliseconds);

        public BeginPeriodSystemTimerStrategy()
        {
            System.Diagnostics.Debug.WriteLine("Starting MM timer");
            BeginPeriod(1);
        }

        public override void Dispose()
        {
            System.Diagnostics.Debug.WriteLine("Ending MM timer");
            EndPeriod(1);
            base.Dispose();
        }
    }
}
