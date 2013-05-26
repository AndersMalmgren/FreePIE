namespace FreePIE.GUI.CodeCompletion.Event.Events
{
    public class PopupStateChanged : IPopupEvent
    {
        private PopupState state;

        public PopupStateChanged(PopupState state)
        {
            this.state = state;
        }

        public EventType Type
        {
            get { return EventType.PopupStateChanged; }
        }

        public EventSource Source
        {
            get { return EventSource.Popup; }
        }

        public object EventArgs
        {
            get { return state; }
        }
    }

    public enum PopupState
    {
        Open,
        Closed
    }
}
