using System;
using System.Collections.Generic;
using System.Diagnostics;
using FreePIE.Core.Contracts;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers.Strategies;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [GlobalEnum]
    [Documentation("Angle unit used by filters.continuousRotation")]
    public enum Units
    {
        [Documentation("Angles in degrees")]
        Degrees = 1,
        [Documentation("Angles in radians")]
        Radians = 2
    }

    [Global(Name = "filters")]
    [Documentation("Filters and helpers for smoothing, scaling and shaping values")]
    public class FilterHelper : IScriptHelper
    {
        private readonly Dictionary<string, double> deltaLastSamples;
        private readonly Dictionary<string, double> simpleLastSamples;
        private readonly Dictionary<string, ContinuesRotationStrategy> continousRotationStrategies;
        private readonly Dictionary<string, Stopwatch> stopwatches; 

        public FilterHelper()
        {
            deltaLastSamples = new  Dictionary<string, double>();
            simpleLastSamples = new Dictionary<string, double>();
            continousRotationStrategies = new Dictionary<string, ContinuesRotationStrategy>();
            stopwatches = new Dictionary<string, Stopwatch>();
        }

        [NeedIndexer]
        [Documentation("Low pass filter. smoothing is between 0 and 1, a higher value gives a smoother but slower response")]
        public double simple(double x, double smoothing, string indexer)
        {
            if(smoothing < 0 || smoothing > 1)
                throw new ArgumentException("Smoothing must be a value between 0 and 1");

            var lastSample = x;
            if (simpleLastSamples.ContainsKey(indexer))
                lastSample = simpleLastSamples[indexer];

            lastSample = (lastSample*smoothing) + (x*(1 - smoothing));
            simpleLastSamples[indexer] = lastSample;

            return lastSample;
        }

        [NeedIndexer]
        [Documentation("Returns how much x has changed since the previous call")]
        public double delta(double x, string indexer)
        {
            var lastSample = x;
            if (deltaLastSamples.ContainsKey(indexer))
                lastSample = deltaLastSamples[indexer];

            deltaLastSamples[indexer] = x;

            return x - lastSample;
        }

        [Deprecated("continuousRotation")]
        [NeedIndexer]
        public double continousRotation(double x, string indexer)
        {
            return continuousRotation(x, indexer);
        }

        [Deprecated("continuousRotation")]
        [NeedIndexer]
        public double continousRotation(double x, Units unit, string indexer)
        {
            return continuousRotation(x, unit, indexer);
        }

        [NeedIndexer]
        [Documentation("Removes the jump when an angle in radians wraps around, so the value keeps increasing past a full turn")]
        public double continuousRotation(double x, string indexer)
        {
            return continuousRotation(x, Units.Radians, indexer);
        }

        [NeedIndexer]
        [Documentation("Removes the jump when an angle wraps around, so the value keeps increasing past a full turn")]
        public double continuousRotation(double x, Units unit, string indexer)
        {
            if(!continousRotationStrategies.ContainsKey(indexer))
                continousRotationStrategies[indexer] = new ContinuesRotationStrategy(unit);

            var strategy = continousRotationStrategies[indexer];
            strategy.Update(x);

            return strategy.Out;
        }

        [Documentation("Applies a dead zone to x in the range minY to maxY and rescales the rest of the range so the output is continuous")]
        public double deadband(double x, double deadZone, double minY, double maxY)
        {
            var scaled = ensureMapRange(x, minY, maxY, -1, 1);
            var y = 0d;

            if (Math.Abs(scaled) > deadZone)
                y = ensureMapRange(Math.Abs(scaled), deadZone, 1, 0, 1) * Math.Sign(x);

            return ensureMapRange(y, -1, 1, minY, maxY);
        }

        [Documentation("Returns 0 while x is within +-deadZone, otherwise x")]
        public double deadband(double x, double deadZone)
        {
            if (Math.Abs(x) >= Math.Abs(deadZone))
                return x;

            return 0;
        }

        [Documentation("Maps x linearly from the range xMin to xMax to the range yMin to yMax")]
        public double mapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return yMin + (yMax - yMin)*(x - xMin)/(xMax - xMin);
        }

        [Documentation("Maps x linearly from the range xMin to xMax to the range yMin to yMax and clamps the result to yMin to yMax")]
        public double ensureMapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return Math.Max(Math.Min(((x - xMin)/(xMax - xMin))*(yMax - yMin) + yMin, yMax), yMin);
        }

        [NeedIndexer]
        [Documentation("Returns true once after state has been true for the given number of milliseconds, resets when state is false")]
        public bool stopWatch(bool state, int milliseconds, string indexer)
        {
            if (!stopwatches.ContainsKey(indexer) && state)
            {
                stopwatches[indexer] = new Stopwatch();
                stopwatches[indexer].Start();
            }

            if (stopwatches.ContainsKey(indexer) && !state)
            {
                stopwatches[indexer].Stop();
                stopwatches.Remove(indexer);
            }

            if (!state) return false;

            var watch = stopwatches[indexer];
            if (watch.ElapsedMilliseconds >= milliseconds)
            {
                watch.Stop();
                stopwatches.Remove(indexer);
                return true;
            }

            return false;
        }
    }
}
