namespace FreePIE.Core.Model.Events
{
    public enum ErrorLevel
    {
        Exception = 1,
        Warning = 2
    }

    public class ScriptErrorEvent
    {
        public ScriptErrorEvent(ErrorLevel level, string description, int? lineNumber)
        {
            Level = level;
            Description = description;
            LineNumber = lineNumber;
        }

        public ErrorLevel Level { get; private set; }
        public string Description { get; private set; }
        public int? LineNumber { get; private set; }
    }
}
