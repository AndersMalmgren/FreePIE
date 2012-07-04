using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class CodeCompletionProvider : ICodeCompletionProvider
    {
        private readonly IScriptParser parser;
        private readonly IRuntimeInfoProvider infoProvider;

        public CodeCompletionProvider(IScriptParser parser, IRuntimeInfoProvider infoProvider)
        {
            this.parser = parser;
            this.infoProvider = infoProvider;
        }

        public IEnumerable<ExpressionInfo> GetSuggestionsForExpression(string script, int offset)
        {
            var tokens = parser.GetTokensFromExpression(script, offset);

            var info = infoProvider.AnalyzeExpression(tokens);

            return info.Select(x => x.Value);
        }
    }
}
