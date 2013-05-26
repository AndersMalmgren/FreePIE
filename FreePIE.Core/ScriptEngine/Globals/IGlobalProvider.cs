using System.Collections.Generic;

namespace FreePIE.Core.ScriptEngine.Globals
{
    public interface IGlobalProvider
    {
        IEnumerable<object> ListGlobals();
    }
}
