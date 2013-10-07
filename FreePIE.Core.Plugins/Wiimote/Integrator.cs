using System;
using System.Diagnostics;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class Integrator
    {
        private readonly Stopwatch stopwatch;
        private double lastSampleTime;

        public Integrator(int numberOfValues)
        {
            Values = new double[numberOfValues];
            stopwatch = new Stopwatch();
        }

        public void Update(double[] differences)
        {
            if(differences.Length != Values.Length)
                throw new InvalidOperationException("Number of differences in the input list must equal the amount of values desired: " + Values.Length);

            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
                return;
            }

            var elapsed = stopwatch.Elapsed.TotalSeconds;
            var timeDiff = elapsed - lastSampleTime;
            lastSampleTime = elapsed;

            for (int i = 0; i < Values.Length; i++)
                Values[i] = Values[i] + (differences[i] * timeDiff);
        }

        public double[] Values { get; private set; }
    }
}