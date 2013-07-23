using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] data;
        private int front;

        public int Size { get; private set; }

        public CircularBuffer(uint capacity)
        {
            data = new T[capacity];
            Size = 0;
            front = -1;
        }

        public void Push(T element)
        {
            front += 1;
            front = front % data.Length;
            data[front] = element;

            Size = Math.Min(data.Length, Size + 1);
        }

        private static uint Modulo(int i, int c)
        {
            var result = i % c;
            return (uint)(result >= 0 ? result : result + c);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int c = 0, i = front; c < Size; c++, i--)
                yield return data[Modulo(i, Size)];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct TimedValue<T>
    {
        public readonly T Value;
        public readonly DateTime Time;

        public TimedValue(DateTime time, T value)
        {
            Time = time;
            Value = value;
        }
    }

    public static class SequenceExtensions
    {
        public static IEnumerable<double> Deltas(this IEnumerable<double> values)
        {
            return values.AdjacentElements().Select(x => x.Item2 - x.Item1);
        }

        public static IEnumerable<Tuple<T, T>> AdjacentElements<T>(this IEnumerable<T> values)
        {
            var previous = values.FirstOrDefault();

            if (default(double).Equals(previous))
                yield break;

            foreach (var value in values.Skip(1))
            {
                yield return new Tuple<T, T>(previous, value);
                previous = value;
            }
        }
    }

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

    public class WiimoteCalibration
    {
        private readonly TimeSeries accelerationMagnitudes;

        private const uint WiimoteStationaryDeltaEpsilon = 1;

        public WiimoteCalibration()
        {
            AccelerationGain = 0;
            AccelerationOffset = 0;
            MotionPlusGainSlow = 1.0 / (8192.0 / 595.0) / 1.44;
            MotionPlusGainFast = MotionPlusGainSlow * 2000 / 440;
            MotionPlusOffset = -0x2000;

            accelerationMagnitudes = new TimeSeries(512);
        }

        private double EuclideanDistance(ushort a, ushort b, ushort c)
        {
            return Math.Sqrt(a*a + b*b + c*c);
        }

        private bool IsStationary()
        {
            return accelerationMagnitudes.Size > 10 && accelerationMagnitudes.DurationStable(WiimoteStationaryDeltaEpsilon) > TimeSpan.FromMilliseconds(1000);
        }

        private void TakeCalibrationSnapshot(ushort accX, ushort accY, ushort accZ)
        {
            AccelerationOffset = (accX + accY) / 2d;
            var gravity = accZ - AccelerationOffset;
            
            AccelerationGain = 9.81 / gravity;
        }

        private static double TransformLinear(double gain, double offset, double value)
        {
            return (value - offset) * gain;
        }

        private bool AccelerationCalibrated { get { return AccelerationGain != 0; } }

        public Acceleration NormalizeAcceleration(DateTime measured, ushort x, ushort y, ushort z)
        {
            accelerationMagnitudes.Add(measured, EuclideanDistance(x, y, z));

            if (IsStationary() && !AccelerationCalibrated)
                TakeCalibrationSnapshot(x, y, z);

            return new Acceleration(TransformLinear(AccelerationGain, AccelerationOffset, x),
                                    TransformLinear(AccelerationGain, AccelerationOffset, y),
                                    TransformLinear(AccelerationGain, AccelerationOffset, z));
        }

        public double AccelerationGain { get; set; }

        public double AccelerationOffset { get; set; }

        public double MotionPlusGainSlow { get; set; }

        public double MotionPlusGainFast { get; set; }

        public double MotionPlusOffset { get; set; }
    }
}
