using System.Linq;

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

            var infos = infoProvider.AnalyzeExpression(tokenResult.Tokens);

            return new CodeCompletionResult(infos, tokenResult.Tokens.Last(), tokenResult.LastTokenRange);
        }

        public bool IsBeginningOfExpression(string script, int caretPosition)
        {
            var tokens = parser.GetTokensFromExpression(script, caretPosition).Tokens;

            if (!tokens.Any())
                return true;

            return tokens.Last().Value.Length == 0;
        }

        public bool IsEndOfExpressionDelimiter(char @char)
        {
            return parser.IsEndOfExpressionDelimiter(@char);
        }
    }
}
