using System;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    
    internal class TimingAttribute : Attribute
    {
        public TimingAttribute(TimingTypes type)
        {
            Type = type;
        }

        public TimingTypes Type { get; set; }
        public bool Default { get; set; }
    }
}
