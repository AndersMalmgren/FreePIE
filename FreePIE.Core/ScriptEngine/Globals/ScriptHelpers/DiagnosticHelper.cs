using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [LuaGlobal(Name = "diagnostics")]
    public class DiagnosticHelper : IScriptHelper
    {
        public void debug(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void debug(object arg)
        {
            Console.WriteLine(arg);
        }
    }
}
