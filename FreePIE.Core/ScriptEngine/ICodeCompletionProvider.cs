using System;
using FreePIE.Core.ScriptEngine.CodeCompletion;

namespace FreePIE.Core.ScriptEngine
{
    public interface ICodeCompletionProvider
    {
        CodeCompletionResult GetSuggestionsForExpression(string script, int offset);
        bool IsBeginningOfExpression(string script, int caretPosition);
        bool IsEndOfExpressionDelimiter(char nextChar);
        SignatureHelpResult GetSignatureHelp(string script, int offset);
        void ScriptChanged(string script, int offset);

        // Raised on a background thread when the script has been analyzed again, suggestions may have changed
        event EventHandler ScriptAnalyzed;
    }
}
