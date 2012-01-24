using System;

namespace FreePIE.Core.ScriptEngine
{
    public interface IScriptEngine
    {
        void Start(string script);
        void Stop();
        event EventHandler<ScriptErrorEventArgs> Error;
    }

    public class ScriptErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public ScriptErrorEventArgs(Exception e)
        {
            this.Exception = e;
        }
    }
}