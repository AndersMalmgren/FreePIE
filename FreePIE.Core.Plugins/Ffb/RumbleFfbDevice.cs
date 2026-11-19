using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FreePIE.Core.Plugins.Ffb
{
    public class RumbleFfbDevice : IFfbSinkDevice, IDisposable
    {
        private readonly Action<double, double> setMotors;
        private readonly Func<double> clock;
        private readonly List<RumbleFfbChannel> channels;
        private double currentLarge;
        private double currentSmall;

        public RumbleFfbDevice(Action<double, double> setMotors) : this(setMotors, CreateClock())
        {
        }

        public RumbleFfbDevice(Action<double, double> setMotors, Func<double> clock)
        {
            this.setMotors = setMotors;
            this.clock = clock;
            channels = new List<RumbleFfbChannel>();
            Gain = 1;
            ActuatorsEnabled = true;
        }

        private static Func<double> CreateClock()
        {
            var stopwatch = Stopwatch.StartNew();
            return () => stopwatch.Elapsed.TotalMilliseconds;
        }

        public double Now
        {
            get { return clock(); }
        }

        public double Gain { get; set; }
        public bool ActuatorsEnabled { get; set; }

        public IFfbSink CreateChannel()
        {
            var channel = new RumbleFfbChannel(this);
            channels.Add(channel);

            return channel;
        }

        public void ReleaseChannel(IFfbSink channel)
        {
            channels.Remove((RumbleFfbChannel) channel);
        }

        // Called every iteration, the strength of time based effects changes even when nothing new is sent
        public void Render()
        {
            double large = 0;
            double small = 0;

            if (ActuatorsEnabled)
            {
                var now = Now;
                channels.ForEach(c => c.Render(now, ref large, ref small));
            }

            SetMotors(Math.Min(1, large * Gain), Math.Min(1, small * Gain));
        }

        private void SetMotors(double large, double small)
        {
            if (large == currentLarge && small == currentSmall)
                return;

            currentLarge = large;
            currentSmall = small;
            setMotors(large, small);
        }

        public void Dispose()
        {
            channels.Clear();
            SetMotors(0, 0);
        }
    }
}
