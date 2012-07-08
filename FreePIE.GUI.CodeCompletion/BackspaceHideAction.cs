using System.Linq;

namespace FreePIE.GUI.CodeCompletion
{
    public class BackspaceHideAction : IPopupAction
    {
        public void Act(EventType type, CompletionPopupView view, object args)
        {
            if (!IsTriggered(type, args, view.Target))
                return;

            PopupViewActions.Hide(view);
        }

        private bool IsTriggered(EventType type, object args, EditorAdapterBase editor)
        {
            if (editor == null || type != EventType.SelectionChanged)
                return false;

            var indexes = Enumerable.Range(editor.CaretIndex - 1, 2).ToList();

            if (editor.Text.Length <= indexes.Last() || indexes.Any(index => index < 0))
                return false;

            return indexes.All(index => char.IsWhiteSpace(editor.Text[index]));
        }
    }
}
