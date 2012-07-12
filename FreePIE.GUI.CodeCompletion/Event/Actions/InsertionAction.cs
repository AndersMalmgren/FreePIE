using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class InsertionAction : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        {
            if (IsTriggered(current, view))
            {
                InsertElement(view);
                current.Cancel();
            }
        }

        private bool IsTriggered(ICancellablePopupEvent current, CompletionPopupView view)
        {
            if (current.Type != EventType.KeyPress || !view.IsOpen)
                return false;

            return ((KeyEventArgs)current.EventArgs).Key == Key.Enter;
        }

        private void InsertElement(CompletionPopupView view)
        {
            if (view.CompletionItems.SelectedItem == null)
                return;

            InsertItem(view.Model.SelectedCompletionItem, view);
        }

        private void InsertItem(ICompletionItem item, CompletionPopupView view)
        {
            item.Insert();
            PopupActions.Hide(view);
        }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        { }
    }
}