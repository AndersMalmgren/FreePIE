using System;

namespace FreePIE.Core.ScriptEngine
{
    public interface IScriptEngine
    {
        void Start(string script, string scriptPath = null);
        void Stop();
    }
}
