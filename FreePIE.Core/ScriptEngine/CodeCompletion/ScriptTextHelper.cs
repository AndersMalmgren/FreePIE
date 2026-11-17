using System;
using System.Linq;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    // Text based helpers for code that is being edited and therefore can not be trusted to parse
    public static class ScriptTextHelper
    {
        private const int MaxLinesToSearchForCall = 10;

        public static int GetLineStart(string script, int offset)
        {
            return offset <= 0 ? 0 : script.LastIndexOf('\n', offset - 1) + 1;
        }

        public static int GetLineEnd(string script, int offset)
        {
            var end = script.IndexOf('\n', offset);
            end = end < 0 ? script.Length : end;

            return end > 0 && script[end - 1] == '\r' ? end - 1 : end;
        }

        public static bool IsInCommentOrString(string script, int offset)
        {
            offset = Math.Min(offset, script.Length);
            var lineStart = GetLineStart(script, offset);
            var masked = Mask(script, lineStart, offset);

            return masked.InComment || masked.InString;
        }

        public static string StripComment(string line)
        {
            var masked = Mask(line, 0, line.Length);
            return masked.CommentStart >= 0 ? line.Substring(0, masked.CommentStart) : line;
        }

        // Finds the opening parenthesis of the call that the offset is inside, returns -1 if not inside a call
        public static int FindOpenCall(string script, int offset, out int argumentIndex)
        {
            argumentIndex = 0;
            offset = Math.Min(offset, script.Length);

            var start = offset;
            for (int i = 0; i < MaxLinesToSearchForCall && start > 0; i++)
                start = GetLineStart(script, start - 1);

            var text = Mask(script, start, offset).Text;
            var depth = 0;

            for (int i = text.Length - 1; i >= 0; i--)
            {
                switch (text[i])
                {
                    case ')':
                    case ']':
                    case '}':
                        depth++;
                        break;
                    case '(':
                        if (depth == 0)
                            return start + i;
                        depth--;
                        break;
                    case '[':
                    case '{':
                        if (depth == 0)
                            return -1;
                        depth--;
                        break;
                    case ',':
                        if (depth == 0)
                            argumentIndex++;
                        break;
                }
            }

            return -1;
        }

        // Replaces strings and comments with spaces so that brackets inside them are ignored
        private static MaskResult Mask(string script, int start, int end)
        {
            var text = script.Substring(start, end - start).ToCharArray();
            var result = new MaskResult { CommentStart = -1 };
            string quote = null;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (result.InComment)
                {
                    if (c == '\n')
                        result.InComment = false;
                    else
                        text[i] = ' ';

                    continue;
                }

                if (quote != null)
                {
                    if (c == '\\' && i + 1 < text.Length)
                    {
                        text[i] = text[i + 1] = ' ';
                        i++;
                        continue;
                    }

                    if (string.CompareOrdinal(new string(text, i, Math.Min(quote.Length, text.Length - i)), quote) == 0 && IsQuoteAt(script, start + i, quote))
                    {
                        i += quote.Length - 1;
                        quote = null;
                        continue;
                    }

                    if (c == '\n' && quote.Length == 1)
                        quote = null;
                    else
                        text[i] = ' ';

                    continue;
                }

                if (c == '#')
                {
                    result.InComment = true;
                    if (result.CommentStart < 0)
                        result.CommentStart = i;
                    text[i] = ' ';
                    continue;
                }

                if (c == '"' || c == '\'')
                {
                    var triple = new string(c, 3);
                    quote = i + 2 < text.Length && text[i + 1] == c && text[i + 2] == c ? triple : c.ToString();
                    i += quote.Length - 1;
                }
            }

            result.InString = quote != null;
            result.Text = new string(text);
            return result;
        }

        private static bool IsQuoteAt(string script, int index, string quote)
        {
            return index + quote.Length <= script.Length && script.Substring(index, quote.Length).All(c => c == quote[0]);
        }

        private class MaskResult
        {
            public string Text { get; set; }
            public bool InComment { get; set; }
            public bool InString { get; set; }
            public int CommentStart { get; set; }
        }
    }
}
