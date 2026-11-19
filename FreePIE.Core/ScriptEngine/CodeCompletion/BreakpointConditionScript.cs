using System;
using System.Linq;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    // A breakpoint condition is evaluated in the scope of the breakpoint line. For code completion the condition
    // is placed in the script as a line of its own just before the breakpoint line, with the same indentation
    public class BreakpointConditionScript
    {
        private readonly string before;
        private readonly string after;

        public BreakpointConditionScript(string script, int lineNumber)
        {
            script = script ?? string.Empty;

            var lineStart = GetLineStart(script, lineNumber);
            if (lineStart < 0)
            {
                before = script + "\n";
                after = string.Empty;
                return;
            }

            var lineEnd = ScriptTextHelper.GetLineEnd(script, lineStart);
            var indent = new string(script.Skip(lineStart).Take(lineEnd - lineStart).TakeWhile(char.IsWhiteSpace).ToArray());
            var contentStart = lineStart + indent.Length;

            before = script.Substring(0, contentStart);
            after = "\n" + indent + script.Substring(contentStart);
        }

        // -1 if the script has fewer lines
        private static int GetLineStart(string script, int lineNumber)
        {
            var start = 0;

            for (var line = 1; line < lineNumber; line++)
            {
                start = script.IndexOf('\n', start) + 1;
                if (start == 0)
                    return -1;
            }

            return start;
        }

        public string GetScript(string condition)
        {
            return before + (condition ?? string.Empty) + after;
        }

        public int ToScriptOffset(int conditionOffset)
        {
            return before.Length + conditionOffset;
        }

        public int ToConditionOffset(int scriptOffset)
        {
            return Math.Max(0, scriptOffset - before.Length);
        }
    }
}
