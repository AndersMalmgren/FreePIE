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
        private readonly Dictionary<string, double> simpleFilterLastSamples;

        public FilterHelper()
        {
            simpleFilterLastSamples = new  Dictionary<string, double>();
        }

        [NeedIndexer]
        public double simple(double x, double smoothing, string indexer)
        {
            if(smoothing < 0 && smoothing > 1)
                throw new ArgumentException("Smooting must be a value between 0 and 1");

            var lastSample = x;
            if (simpleFilterLastSamples.ContainsKey(indexer))
                lastSample = simpleFilterLastSamples[indexer];

            lastSample = (lastSample*smoothing) + (x*(1 - smoothing));
            simpleFilterLastSamples[indexer] = lastSample;

            return lastSample;
        }
    }
}
