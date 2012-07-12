using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using FreePIE.GUI.CodeCompletion.Controls;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    class InsertOnItemClicked : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        void InsertItem(ICompletionItem item)
        {
            if(item == null)
                throw new InvalidOperationException("ICompletionItem is null. Something is wrong with the hackish ItemClicked event.");

            item.Insert();
        }

        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        {
            if (current.Source != EventSource.Popup || current.Type != EventType.ItemClicked)
                return;

            var eventArgs = current.EventArgs as ItemClickedEventArgs;

            InsertItem(eventArgs.CompletionItem);

            PopupActions.Hide(view);

            current.Cancel();
        }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        { }
    }
}
