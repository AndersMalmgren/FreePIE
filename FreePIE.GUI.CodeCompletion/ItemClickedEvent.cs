using System.Windows.Input;
using FreePIE.GUI.CodeCompletion.Event;

namespace FreePIE.GUI.CodeCompletion
{
    public class ItemClickedEvent : ICancellablePopupEvent
    {
        private readonly ItemClickedEventArgs args;

        public ItemClickedEvent(MouseEventArgs args, ICompletionItem item)
        {
            this.args = new ItemClickedEventArgs(args, item);
        }

        public EventType Type
        {
            get { return EventType.ItemClicked; }
        }

        public EventSource Source
        {
            get { return EventSource.Popup; }
        }

        public object EventArgs
        {
            get { return args; }
        }

        public void Cancel()
        {
            args.Args.Handled = true;
        }

        public bool IsCancelled
        {
            get { return args.Args.Handled; }
        }
    }
}