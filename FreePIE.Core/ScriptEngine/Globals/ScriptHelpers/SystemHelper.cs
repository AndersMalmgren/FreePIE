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
    [Documentation("Controls how the script is executed")]
    public class SystemHelper : IScriptHelper
    {
        private readonly IThreadTimingFactory threadTimingFactory;

        public SystemHelper(IThreadTimingFactory threadTimingFactory)
        {
            this.threadTimingFactory = threadTimingFactory;
        }

        [Documentation("Sets how the script thread waits between iterations, see TimingTypes")]
        public void setThreadTiming(TimingTypes timing)
        {
            threadTimingFactory.Set(timing);
        }

        [Documentation("Time to wait between script iterations, in milliseconds (microseconds with ThreadYieldMicroSeconds)")]
        public int threadExecutionInterval
        {
            get { return threadTimingFactory.Get().ThreadExecutionInterval; }
            set { threadTimingFactory.Get().ThreadExecutionInterval = value; }
        }
    }
}
