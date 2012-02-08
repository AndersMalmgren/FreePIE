using System.Collections.Generic;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.ScriptEngine
{
    public interface IScriptParser
    {
        IEnumerable<IPlugin> InvokeAndConfigureAllScriptDependantPlugins(string script);
        string PrepareScript(string script, IEnumerable<object> globals);
    }
}