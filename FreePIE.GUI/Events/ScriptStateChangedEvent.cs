namespace FreePIE.GUI.Events
{
    public class ScriptStateChangedEvent
    {
        public bool Running { get; set; }

        public string Script { get; set; }
        public ScriptStateChangedEvent(bool running, string script)
        {
            Running = running;
            Script = script;
        }
    }
}
