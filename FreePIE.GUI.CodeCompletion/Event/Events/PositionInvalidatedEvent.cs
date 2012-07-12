using System;
using FreePIE.GUI.CodeCompletion.Event;

namespace FreePIE.GUI.CodeCompletion
{
    internal class PositionInvalidatedEvent : IPopupEvent
    {
        public EventType Type
        {
            get { return EventType.PositionInvalidated; }
        }

        public EventSource Source
        {
            get { return EventSource.Popup; }
        }

        public object EventArgs
        {
            get { throw new InvalidOperationException("No state associated with this event."); }
        }
    }
}