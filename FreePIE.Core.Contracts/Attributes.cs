using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Contracts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GlobalType : Attribute
    {
        public Type Type { get; set; }

        public bool IsIndexed { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Global : Attribute
    {
        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Enum)]
    public class GlobalEnum : Attribute { }
}
