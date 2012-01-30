using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Contracts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LuaGlobalType : Attribute
    {
        public Type Type { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class LuaGlobal : Attribute
    {
        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Enum)]
    public class LuaGlobalEnum : Attribute { }
}
