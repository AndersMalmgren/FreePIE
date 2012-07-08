namespace FreePIE.GUI.CodeCompletion
{
    public class SelectionChangedHideAction : IPopupAction
    {

        private int lastIndex;

        public void Act(EventType type, CompletionPopupView view, object args)
        {
            if (!IsTriggered(type, args, view.Target))
                return;

            int caretIndex = (int)args;
            lastIndex = caretIndex;
        }

        private bool IsTriggered(EventType type, object args, EditorAdapterBase editor)
        {
            if (type != EventType.SelectionChanged || editor == null)
                return false;

            int caretIndex = (int)args;

            return !editor.IsSameLine(caretIndex, lastIndex);
        }
    }
}
