using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model.Events;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [LuaGlobal(Name = "diagnostics")]
    public class DiagnosticHelper : IScriptHelper
    {
        private readonly IEventAggregator eventAggregator;

        public DiagnosticHelper(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void debug(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void debug(object arg)
        {
            Console.WriteLine(arg);
        }

        [NeedIndexer]
        public void watch(object value, string indexer)
        {
            eventAggregator.Publish(new WatchEvent(indexer, value));
        }
    }
}
