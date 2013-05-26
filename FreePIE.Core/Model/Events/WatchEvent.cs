namespace FreePIE.Core.Model.Events
{
    public class WatchEvent
    {
        public WatchEvent(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public object Value { get; set; }
    }
}
