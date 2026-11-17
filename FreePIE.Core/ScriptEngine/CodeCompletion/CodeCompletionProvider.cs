using System;
using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class CodeCompletionProvider : ICodeCompletionProvider
    {
        private readonly IScriptParser parser;
        private readonly IScriptInfoProvider infoProvider;

        public CodeCompletionProvider(IScriptParser parser, IScriptInfoProvider infoProvider)
        {
            this.parser = parser;
            this.infoProvider = infoProvider;
            infoProvider.ScriptAnalyzed += (sender, args) => OnScriptAnalyzed();
        }

        public event EventHandler ScriptAnalyzed;

        private void OnScriptAnalyzed()
        {
            var handler = ScriptAnalyzed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public CodeCompletionResult GetSuggestionsForExpression(string script, int offset)
        {
            script = script ?? string.Empty;

            var tokenResult = parser.GetTokensFromExpression(script, offset);
            var tokens = tokenResult.Tokens.ToList();

            var infos = ScriptTextHelper.IsInCommentOrString(script, offset)
                ? Enumerable.Empty<ExpressionInfo>()
                : Distinct(infoProvider.AnalyzeExpression(script, offset, tokens));

            return new CodeCompletionResult(infos.ToList(), tokens.Last(), tokenResult.LastTokenRange);
        }

        // Script names shadows plugins and builtins with the same name, overloads are shown once
        private static IEnumerable<ExpressionInfo> Distinct(IEnumerable<ExpressionInfo> infos)
        {
            return infos
                .GroupBy(i => i.Name)
                .Select(MergeOverloads)
                .OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase);
        }

        // The infos are shared by the runtime info tree, so a new info is created instead of changing the first overload
        private static ExpressionInfo MergeOverloads(IEnumerable<ExpressionInfo> group)
        {
            var first = group.First();
            var overloads = group.Where(i => i.Kind == first.Kind && i.Signature != null).Select(i => i.Signature).ToList();
            if (first.Signature == null || overloads.Count < 2)
                return first;

            var signatures = string.Join(Environment.NewLine, overloads.Select(s => s.ToString()));
            var documentation = string.Join(Environment.NewLine, overloads.Select(s => s.Documentation).Where(d => d.Length > 0).Distinct());

            return new ExpressionInfo
            {
                Name = first.Name,
                Kind = first.Kind,
                Signature = first.Signature,
                Description = ExpressionInfo.WithDocumentation(signatures, documentation)
            };
        }

        public void ScriptChanged(string script, int offset)
        {
            infoProvider.ScriptChanged(script, offset);
        }

        public SignatureHelpResult GetSignatureHelp(string script, int offset)
        {
            script = script ?? string.Empty;

            int argumentIndex;
            var openParenthesis = ScriptTextHelper.FindOpenCall(script, offset, out argumentIndex);
            if (openParenthesis < 0)
                return null;

            var tokens = parser.GetTokensFromExpression(script, openParenthesis).Tokens.ToList();
            var name = tokens.Last().Value;
            if (name.Length == 0)
                return null;

            var signatures = infoProvider.AnalyzeExpression(script, openParenthesis, tokens)
                .Where(i => i.Name == name && i.Signature != null)
                .Select(i => i.Signature)
                .ToList();

            return signatures.Any() ? new SignatureHelpResult(signatures, argumentIndex, openParenthesis) : null;
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
