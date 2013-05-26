﻿using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Events
{
    public class CancellableKeyEvent : KeyEvent, ICancellablePopupEvent
    {
        public CancellableKeyEvent(KeyEventArgs args, EventSource source) : base(args, source)
        { }

        public void Cancel()
        {
            args.Handled = true;
        }

        public bool IsCancelled
        {
            get { return args.Handled; }
        }

        public bool IsTransient
        {
            get { return true; }
        }
    }

    public class KeyUpEvent : KeyEvent
    {
        public KeyUpEvent(KeyEventArgs args, EventSource source) : base(args, source)
        { }

        public override EventType Type
        {
            get
            {
                return EventType.KeyUp;
            }
        }
    }

    public class KeyEvent : IPopupEvent
    {
        protected readonly KeyEventArgs args;
        protected readonly EventSource source;

        public KeyEvent(KeyEventArgs args, EventSource source)
        {
            this.args = args;
            this.source = source;
        }

        public virtual EventType Type
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
