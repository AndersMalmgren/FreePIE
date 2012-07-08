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

        public CodeCompletionResult GetSuggestionsForExpression(string script, int offset)
        {
            var tokenResult = parser.GetTokensFromExpression(script, offset);

            var info = infoProvider.AnalyzeExpression(tokenResult.Tokens);

            return new CodeCompletionResult(info.Select(x => x.Value), tokenResult.LastToken);
        }
    }
}
