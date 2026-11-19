using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    [Timing(TimingTypes.HighResWaitableTimer)]
    public class HighResWaitableTimer : Timing, IDisposable
    {
        private const uint CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x00000002;
        private const uint TIMER_ALL_ACCESS = 0x1F0003;
        private const uint INFINITE = 0xFFFFFFFF;

        private readonly IntPtr timerHandle;
        private readonly Stopwatch stopwatch;
        private long targetTicks;

        public HighResWaitableTimer()
        {
            timerHandle = CreateWaitableTimerExW(
                IntPtr.Zero,
                null,
                CREATE_WAITABLE_TIMER_HIGH_RESOLUTION,
                TIMER_ALL_ACCESS);

            if (timerHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create high-resolution waitable timer.");

            stopwatch = Stopwatch.StartNew();
        }

        public override void Wait()
        {
            // ThreadExecutionInterval is specified in milliseconds
            long intervalTicks = (long)(ThreadExecutionInterval * (Stopwatch.Frequency / 1000.0));
            targetTicks += intervalTicks;

            long currentTicks = stopwatch.ElapsedTicks;
            long remainingTicks = targetTicks - currentTicks;

            // If the script iteration overran its budget, reset the target to avoid burst catch-up
            if (remainingTicks <= 0)
            {
                targetTicks = currentTicks;
                return;
            }

            // Convert Stopwatch ticks to 100-nanosecond intervals (negative = relative time)
            long dueTime100Ns = -(remainingTicks * 10_000_000L / Stopwatch.Frequency);

            if (dueTime100Ns < 0)
            {
                SetWaitableTimer(timerHandle, ref dueTime100Ns, 0, IntPtr.Zero, IntPtr.Zero, false);
                WaitForSingleObject(timerHandle, INFINITE);
            }
        }

        public void Dispose()
        {
            if (timerHandle != IntPtr.Zero)
                CloseHandle(timerHandle);
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWaitableTimerExW(
            IntPtr lpTimerAttributes,
            string lpTimerName,
            uint dwFlags,
            uint dwDesiredAccess);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetWaitableTimer(
            IntPtr hTimer,
            [In] ref long pDueTime,
            int lPeriod,
            IntPtr pfnCompletionRoutine,
            IntPtr lpArgToCompletionRoutine,
            bool fResume);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}