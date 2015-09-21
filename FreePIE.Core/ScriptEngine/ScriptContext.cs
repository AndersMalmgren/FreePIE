using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.ScriptEngine
{
    internal class ScriptContext : IScriptContext, IScriptContextController, IDisposable
    {
        public event EventHandler BeforeScriptExecuting;

        public ScriptContext()
        {
            Debug.WriteLine("Creating script context");
        }

        public virtual void OnBeforeScriptExecuting(object sender, EventArgs args)
        {
            if (BeforeScriptExecuting != null)
                BeforeScriptExecuting(sender, args);
        }

        public void Dispose()
        {
            Debug.WriteLine("{0} Disposing script context", GetHashCode());
        }

        ~ScriptContext()
        {
            Debug.WriteLine("GC script context");
        }
    }
}
