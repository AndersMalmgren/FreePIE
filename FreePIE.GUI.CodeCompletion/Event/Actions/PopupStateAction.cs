using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.GUI.CodeCompletion.Event.Events;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    // Notifies when the popup is opened or closed
    public class PopupStateAction : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        private readonly Action<bool> isOpenChanged;

        public PopupStateAction(Action<bool> isOpenChanged)
        {
            this.isOpenChanged = isOpenChanged;
        }

        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        { }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        {
            var current = events.FirstOrDefault();

            if (current == null || current.Type != EventType.PopupStateChanged)
                return;

            isOpenChanged((PopupState)current.EventArgs == PopupState.Open);
        }
    }
}
