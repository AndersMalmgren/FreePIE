using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.GUI.Events
{
    public abstract class ScriptEvent
    {
        public string Script { get; set; }

        public ScriptEvent(string script)
        {
            Script = script;
        }
    }
}
