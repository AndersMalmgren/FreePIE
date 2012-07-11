using System.Collections.Generic;
using System.Linq;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class BackspaceHideAction : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        public void Act(EventType type, CompletionPopupView view, object args)
        {

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

        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        { }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        {
            var current = events.First();

            if (!IsTriggered(current.Type, current.EventArgs, view.Target))
                return;

            PopupActions.Hide(view);
        }
    }
}
