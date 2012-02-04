using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.GUI.Events
{
    public class ScriptStateChangedEvent
    {
        public bool Running { get; set; }
        public ScriptStateChangedEvent(bool running)
        {
            Running = running;
        }
    }
}
