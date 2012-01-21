using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.GUI.Events
{
    public class ScriptUpdatedEvent
    {
        public string Script { get; set; }

        public ScriptUpdatedEvent(string script)
        {
            Script = script;
        }
    }
}
