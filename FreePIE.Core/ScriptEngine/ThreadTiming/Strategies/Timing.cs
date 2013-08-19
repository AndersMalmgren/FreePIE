using System;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    public abstract class Timing : IDisposable
    {
        public int ThreadExecutionInterval { get; set; }
        public abstract void Wait();

        public virtual void Dispose()
        {
        }
    }
}
