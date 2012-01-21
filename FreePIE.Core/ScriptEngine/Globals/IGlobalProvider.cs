using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.ScriptEngine.Globals
{
    public interface IGlobalProvider
    {
        IEnumerable<object> ListGlobals();
    }
}
