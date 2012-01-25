using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [LuaGlobal(Name = "filters")]
    public class FilterHelper : IScriptHelper
    {
        private readonly Dictionary<string, double> lastSamples;

        public FilterHelper()
        {
            lastSamples = new  Dictionary<string, double>();
        }

        [NeedIndexer]
        public double simple(double x, double smoothing, string indexer)
        {
            if(smoothing < 0 && smoothing > 1)
                throw new ArgumentException("Smoothing must be a value between 0 and 1");

            var lastSample = x;
            if (lastSamples.ContainsKey(indexer))
                lastSample = lastSamples[indexer];

            lastSample = (lastSample*smoothing) + (x*(1 - smoothing));
            lastSamples[indexer] = lastSample;

            return lastSample;
        }

        [NeedIndexer]
        public double delta(double x, string indexer)
        {
            var lastSample = x;
            if (lastSamples.ContainsKey(indexer))
                lastSample = lastSamples[indexer];

            lastSamples[indexer] = x;

            return lastSample - x;
        }
    }
}
