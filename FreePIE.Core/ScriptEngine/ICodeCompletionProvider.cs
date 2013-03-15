﻿using FreePIE.Core.ScriptEngine.CodeCompletion;

namespace FreePIE.Core.ScriptEngine
{
    public interface ICodeCompletionProvider
    {
        CodeCompletionResult GetSuggestionsForExpression(string script, int offset);
        bool IsBeginningOfExpression(string script, int caretPosition);
        bool IsEndOfExpressionDelimiter(char nextChar);
    }
}
