using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion
{
    public class ElementChangedKeyAction : KeyAction
    {
        protected override void DoAct(CompletionPopupView view, KeyEventArgs args)
        {
            view.PerformElementChanged(args);
        }
    }
}
