namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public static class PopupActions
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
