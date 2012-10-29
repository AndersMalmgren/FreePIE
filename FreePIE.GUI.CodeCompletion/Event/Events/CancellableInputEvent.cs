using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Events
{
    public class CancellableInputEvent : ICancellablePopupEvent
    {
        private TextCompositionEventArgs args;

        public CancellableInputEvent(TextCompositionEventArgs args)
        {
            this.args = args;
        }

        public EventType Type { get { return EventType.CancellableInput; } }
        public EventSource Source { get { return EventSource.Editor; } }
        public object EventArgs { get { return args; } }
        public void Cancel()
        {
            args.Handled = true;
        }

        public bool IsCancelled { get { return args.Handled; } }

        public bool IsTransient { get { return true; } }
    }
}