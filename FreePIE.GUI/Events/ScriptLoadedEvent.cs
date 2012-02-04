using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.GUI.Events
{
    public class ScriptLoadedEvent: ScriptEvent
    {
        public ScriptLoadedEvent(string script) : base(script) { }
    }
}
