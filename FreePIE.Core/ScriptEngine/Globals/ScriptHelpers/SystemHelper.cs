using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using FreePIE.Core.ScriptEngine.ThreadTiming;
using FreePIE.Core.ScriptEngine.ThreadTiming.Strategies;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [Global(Name = "system")]
    public class SystemHelper : IScriptHelper
    {
        private readonly IThreadTimingFactory threadTimingFactory;

        public SystemHelper(IThreadTimingFactory threadTimingFactory)
        {
            this.threadTimingFactory = threadTimingFactory;
        }

        public void setThreadTiming(TimingTypes timing)
        {
            threadTimingFactory.Set(timing);
        }
    }
}
