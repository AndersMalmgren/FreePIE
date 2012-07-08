namespace FreePIE.GUI.CodeCompletion
{
    public class ActResult
    {
        public ActResult(bool isTriggered)
        {
            this.IsTriggered = isTriggered;
        }

        public bool IsTriggered { get; private set; }
    }

    public enum EventType
    {
        KeyPress,
        SelectionChanged
    }

    public interface IPopupAction
    {
        void Act(EventType type, CompletionPopupView view, object args);
    }

    public static class PopupViewActions
    {
        public static void Hide(CompletionPopupView view)
        {
            view.IsOpen = false;
            view.Target.Focus();
        }

        public static void ForceShow(CompletionPopupView view)
        {
            if (view.IsOpen)
                view.IsOpen = false;
            view.IsOpen = true;
        }

        public static void Show(CompletionPopupView view)
        {
            if(view.CompletionElements.HasItems)
                ForceShow(view);
        }
    }
}
