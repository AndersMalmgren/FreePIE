using System;
using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class TimeSeries
    {
        private readonly CircularBuffer<TimedValue<double>> buffer;

        public int Size
        {
            get { return buffer.Size; }
        }

        public TimeSeries(uint capacity)
        {
            buffer = new CircularBuffer<TimedValue<double>>(capacity);
        }

        public void Add(DateTime time, double value)
        {
            buffer.Push(new TimedValue<double>(time, value));
        }

        private IEnumerable<TimedValue<double>> DeltaBelowEpsilon(double epsilon)
        {
            var latest = buffer.First();

            foreach (var timedValue in buffer.Skip(1))
                if (Math.Abs(timedValue.Value - latest.Value) < epsilon)
                    yield return timedValue;
                else yield break;
        }

        public TimeSpan DurationStable(double epsilon)
        {
            var filtered = DeltaBelowEpsilon(epsilon).ToList();
            var deltas = filtered.Select(t => t.Time).AdjacentElements().Select(x => x.Item1 - x.Item2);
            return deltas.Aggregate(TimeSpan.FromMilliseconds(0), (current, pair) => current + (pair));
        }

        public double Average(double epsilon)
        {
            return DeltaBelowEpsilon(epsilon).Select(t => t.Value).Average();
        }
    }
}