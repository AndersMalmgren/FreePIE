using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.GUI.Events
{
    public class ScriptErrorEvent
    {
        public Exception Exception { get; set; }
        public int? LineNumber { get; set; }

        public ScriptErrorEvent(Exception e, int? lineNumber)
        {
            Exception = e;
            LineNumber = lineNumber;
        }
    }
}
