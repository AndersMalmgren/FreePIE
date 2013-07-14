using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers.Strategies;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [GlobalEnum]
    public enum Units
    {
        Degrees = 1,
        Radians = 2
    }

    [Global(Name = "filters")]
    public class FilterHelper : IScriptHelper
    {
        private readonly Dictionary<string, double> deltaLastSamples;
        private readonly Dictionary<string, double> simpleLastSamples;
        private readonly Dictionary<string, ContinuesRotationStrategy> continousRotationStrategies;

        public FilterHelper()
        {
            deltaLastSamples = new  Dictionary<string, double>();
            simpleLastSamples = new Dictionary<string, double>();
            continousRotationStrategies = new Dictionary<string, ContinuesRotationStrategy>();
        }

        [NeedIndexer]
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
        public double delta(double x, string indexer)
        {
            var lastSample = x;
            if (deltaLastSamples.ContainsKey(indexer))
                lastSample = deltaLastSamples[indexer];

            deltaLastSamples[indexer] = x;

            return x - lastSample;
        }

        [NeedIndexer]
        public double continousRotation(double x, string indexer)
        {
            return continousRotation(x, Units.Radians, indexer);
        }

        [NeedIndexer]
        public double continousRotation(double x, Units unit, string indexer)
        {
            if(!continousRotationStrategies.ContainsKey(indexer))
                continousRotationStrategies[indexer] = new ContinuesRotationStrategy(unit);

            var strategy = continousRotationStrategies[indexer];
            strategy.Update(x);

            return strategy.Out;
        }
    }
}
