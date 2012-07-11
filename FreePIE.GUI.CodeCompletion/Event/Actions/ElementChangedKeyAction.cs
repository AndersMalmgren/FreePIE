using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class ElementChangedKeyAction : KeyAction
    {
        protected override void DoAct(CompletionPopupView view, KeyEventArgs args)
        {
            view.PerformElementChanged(args);
        }

        protected override bool IsTriggeredAddon(IPopupEvent @event, CompletionPopupView view)
        {
            return view.IsOpen;
        }
    }
}
