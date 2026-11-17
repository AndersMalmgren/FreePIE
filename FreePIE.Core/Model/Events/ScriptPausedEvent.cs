namespace FreePIE.Core.Model.Events
{
    public class ScriptPausedEvent
    {
        public ScriptPausedEvent(int lineNumber)
        {
            LineNumber = lineNumber;
        }

        public int LineNumber { get; private set; }
    }
}
