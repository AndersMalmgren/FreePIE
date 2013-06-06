using System.Diagnostics;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class SamplePeriodCounter
    {
        private readonly Stopwatch stopwatch;
        private uint samples;

        public SamplePeriodCounter()
        {
            stopwatch = new Stopwatch();
        }

        public bool Update()
        {
            if (!stopwatch.IsRunning)
            {
                stopwatch.Restart();
                return false;
            }

            samples++;

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                stopwatch.Stop();
                SamplePeriod = 1 / (float)(samples / stopwatch.Elapsed.TotalSeconds);
                return true;
            }

            return false;
        }

        public void Stop()
        {
            stopwatch.Stop();
        }

        public float SamplePeriod { get; private set; }
    }
}