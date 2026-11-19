using System;

namespace FreePIE.GUI.Common.AvalonEdit
{
    public class BreakpointConditionRequestedEventArgs : EventArgs
    {
        public BreakpointConditionRequestedEventArgs(int lineNumber)
        {
            LineNumber = lineNumber;
        }

        public int LineNumber { get; private set; }
    }
}
