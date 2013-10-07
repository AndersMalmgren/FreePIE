using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.ScriptEngine.ThreadTiming.Strategies;

namespace FreePIE.Core.ScriptEngine.ThreadTiming
{
    public interface IThreadTimingFactory
    {
        void SetDefault();
        Timing Get();
        void Set(TimingTypes type);
    }
}
