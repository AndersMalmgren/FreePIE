namespace FreePIE.Core.ScriptEngine
{
    public class Breakpoint
    {
        public Breakpoint(int lineNumber, string condition = null)
        {
            LineNumber = lineNumber;
            Condition = string.IsNullOrWhiteSpace(condition) ? null : condition.Trim();
        }

        public int LineNumber { get; private set; }

        // A Python expression, the breakpoint only breaks when it is true. Null for a breakpoint that always breaks
        public string Condition { get; private set; }

        public bool HasCondition
        {
            get { return Condition != null; }
        }
    }
}
