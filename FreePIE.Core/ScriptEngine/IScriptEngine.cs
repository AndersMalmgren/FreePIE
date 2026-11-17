using System;

namespace FreePIE.Core.ScriptEngine
{
    public interface IScriptEngine
    {
        IScriptDebugger Debugger { get; }
        void Start(string script, string scriptPath = null);
        void Stop();
    }
}
