using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class ElementChangedKeyAction : KeyAction
    {
        protected override void DoAct(CompletionPopupView view, KeyEventArgs args)
        {
            PerformElementChanged(view, args);
        }

        private void PerformElementChanged(CompletionPopupView view, KeyEventArgs args)
        {
            if (view.CompletionItems.Items.Count <= 0)
                return;

            if (args.Key == Key.Down)
                view.Model.SelectNextCompletionItem();
            else view.Model.SelectPreviousCompletionItem();

            view.CompletionItems.ScrollIntoView(view.Model.SelectedCompletionItem);
        }

        protected override bool ShouldSwallow(CompletionPopupView view, KeyEventArgs args)
        {
            return view.CompletionItems.Items.Count > 0;
        }

        protected override bool IsTriggeredAddon(IPopupEvent @event, CompletionPopupView view)
        {
            return view.IsOpen;
        }
    }
}
