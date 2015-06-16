using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.ScriptEngine
{
    internal class ScriptContext : IScriptContext, IScriptContextController
    {
        public event EventHandler BeforeScriptExecuting;

        public virtual void OnBeforeScriptExecuting(object sender, EventArgs args)
        {
            if (BeforeScriptExecuting != null)
                BeforeScriptExecuting(sender, args);
        }
    }
}
