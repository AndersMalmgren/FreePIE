using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class InsertionAction : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        {
            if (IsTriggered(current))
            {
                InsertElement(view);
                current.Cancel();
            }
        }

        private bool IsTriggered(ICancellablePopupEvent current)
        {
            if (current.Type != EventType.KeyPress || current.Source != EventSource.Popup)
                return false;

            return ((KeyEventArgs)current.EventArgs).Key == Key.Enter;
        }

        private void InsertElement(CompletionPopupView view)
        {
            if (view.CompletionElements.SelectedItem == null)
                return;

            InsertItem(view.CompletionElements.SelectedItem as ICompletionItem, view);
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