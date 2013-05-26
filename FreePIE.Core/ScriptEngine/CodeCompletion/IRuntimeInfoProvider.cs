using System.Collections.Generic;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public interface IRuntimeInfoProvider
    {
        IEnumerable<ExpressionInfo> AnalyzeExpression(IEnumerable<Token> tokens);
    }
}
