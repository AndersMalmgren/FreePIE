using System;
using FreePIE.GUI.CodeCompletion.Event;

namespace FreePIE.GUI.CodeCompletion
{
    public class SelectionChangedEvent : IPopupEvent
    {
        public SelectionChangedEvent(int offset)
        {
            this.EventArgs = offset;
        }

        public EventType Type
        {
            get { return EventType.SelectionChanged; }
        }

        public EventSource Source
        {
            get { return EventSource.Editor; }
        }

        public object EventArgs { get; private set; }
    }
}