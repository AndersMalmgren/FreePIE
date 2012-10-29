using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.ScriptEngine.CodeCompletion;

namespace FreePIE.Core.ScriptEngine
{
    public interface IScriptParser
    {
        IEnumerable<IPlugin> InvokeAndConfigureAllScriptDependantPlugins(string script);
        TokenResult GetTokensFromExpression(string script, int offset);
        string PrepareScript(string script, IEnumerable<object> globals);
        bool IsEndOfExpressionDelimiter(char @char);
    }
}