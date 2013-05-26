using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Model.Events
{
    public class ScriptErrorEvent
    {
        public ScriptErrorEvent(Exception exception, int? lineNumber)
        {
            Exception = exception;
            LineNumber = lineNumber;
        }

        public Exception Exception { get; private set; }
        public int? LineNumber { get; private set; }
    }
}
