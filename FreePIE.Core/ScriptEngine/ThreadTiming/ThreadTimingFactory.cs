using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Common;
using FreePIE.Core.ScriptEngine.ThreadTiming.Strategies;

namespace FreePIE.Core.ScriptEngine.ThreadTiming
{
    class ThreadTimingFactory : IThreadTimingFactory
    {
        private readonly IFactory<Timing> timingFactory;
        private Timing timing;
        private readonly IDictionary<TimingAttribute, Type> timingStrategyTypes;

        public ThreadTimingFactory(IFactory<Timing> timingFactory)
        {
            this.timingFactory = timingFactory;
            timingStrategyTypes = Utils.GetAttributeImplementations<TimingAttribute>();
        }
        
        public void SetDefault()
        {
            Set(timingStrategyTypes.Keys.Single(k => k.Default).Type);
        }

        public Timing Get()
        {
            return timing;
        }

        public void Set(TimingTypes type)
        {
            var keyValue = timingStrategyTypes.Single(k => k.Key.Type == type);
            timing = timingFactory.Create(keyValue.Value);
            timing.ThreadExecutionInterval = 1;
        }
    }
}