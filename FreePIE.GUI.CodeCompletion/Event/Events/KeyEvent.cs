using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Events
{
    public class KeyEvent : ICancellablePopupEvent
    {
        private KeyEventArgs args;
        private readonly EventSource source;

        public KeyEvent(KeyEventArgs args, EventSource source)
        {
            this.args = args;
            this.source = source;
        }

        public void Cancel()
        {
            args.Handled = true;
        }

        public bool IsCancelled
        {
            get { return args.Handled; }
        }

        public EventType Type
        {
            get { return EventType.KeyPress; }
        }

        public object EventArgs
        {
            get { return args; }
        }

        public EventSource Source
        {
            get { return source; }
        }
    }
}
