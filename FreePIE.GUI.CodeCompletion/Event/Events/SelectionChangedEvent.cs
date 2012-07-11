using System;
using FreePIE.GUI.CodeCompletion.Event;

namespace FreePIE.GUI.CodeCompletion
{
    public class SelectionChangedEvent : IPopupEvent
    {
        public EventType Type
        {
            get { return EventType.SelectionChanged; }
        }

        public EventSource Source
        {
            get { return EventSource.Editor; }
        }

        public object EventArgs
        {
            get { throw new InvalidOperationException("No state associated with SelectionChangedEvent"); }
        }
    }
}