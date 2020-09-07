using System;
using System.Collections.Generic;

namespace FreePIE.Core.ScriptEngine
{
    public interface IScriptEngine
    {
        void Start(string script, List<string> additionalPaths);
        void Stop();
    }
}
