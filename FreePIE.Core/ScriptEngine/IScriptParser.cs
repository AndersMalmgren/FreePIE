using System.Collections.Generic;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.ScriptEngine
{
    public interface IScriptParser
    {
        IEnumerable<IOPlugin> InvokeAndConfigureAllScriptDependantPlugins(string script);
        string FindAndInitMethodsThatNeedIndexer(string script, IEnumerable<object> globals);
    }
}