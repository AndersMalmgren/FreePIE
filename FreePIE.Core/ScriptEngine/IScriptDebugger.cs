using System.Collections.Generic;

namespace FreePIE.Core.ScriptEngine
{
    // Line numbers are the line numbers of the script as it is written in the editor (starting at 1)
    public interface IScriptDebugger
    {
        bool IsPaused { get; }
        void SetBreakpoints(IEnumerable<Breakpoint> breakpoints);
        void Continue();
        void StepInto();
        void StepOver();
        void StepOut();

        // Returns a description of the value of the expression at index in the script, null if there is nothing to show
        string Evaluate(string script, int index);
    }
}
