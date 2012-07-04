using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.ScriptEngine.CodeCompletion;

namespace FreePIE.Core.ScriptEngine
{
    public interface ICodeCompletionProvider
    {
        IEnumerable<ExpressionInfo> GetSuggestionsForExpression(string script, int offset);
    }
}
