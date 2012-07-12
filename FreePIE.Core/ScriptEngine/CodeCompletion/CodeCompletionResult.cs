using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Common;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class CodeCompletionResult
    {
        public CodeCompletionResult(IEnumerable<ExpressionInfo> expressionInfos, Token activeToken, Range replaceRange)
        {
            ExpressionInfos = expressionInfos;
            ActiveToken = activeToken;
            ReplaceRange = replaceRange;
        }

        public IEnumerable<ExpressionInfo> ExpressionInfos { get; private set; }
        public Range ReplaceRange { get; private set; }
        public Token ActiveToken { get; private set; }
    }
}
