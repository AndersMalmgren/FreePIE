using System;

namespace FreePIE.Core.ScriptEngine
{
    public interface IScriptContextController
    {
        void OnBeforeScriptExecuting(object sender, EventArgs args);
    }
}