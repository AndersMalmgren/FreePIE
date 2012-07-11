using System.Windows;
using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class ElementChangedKeyAction : KeyAction
    {
        protected override void DoAct(CompletionPopupView view, KeyEventArgs args)
        {
            PerformElementChanged(view, args);
        }

        public void PerformElementChanged(CompletionPopupView view, KeyEventArgs args)
        {
            if (view.CompletionElements.Items.Count <= 0)
                return;

            FocusFirstElement(view);
            view.CompletionElements.RaiseEvent(args);
        }

        public void FocusFirstElement(CompletionPopupView view)
        {
            view.CompletionElements.Focus();
            (view.CompletionElements.ItemContainerGenerator.ContainerFromIndex(0) as UIElement).Focus();
        }

        protected override bool IsTriggeredAddon(IPopupEvent @event, CompletionPopupView view)
        {
            return view.IsOpen;
        }
    }
}
