using System.Collections.Generic;
using System.Linq;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class SelectionChangedHideAction : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        private int lastIndex;

        private bool IsTriggered(EventType type, object args, EditorAdapterBase editor)
        {
            if (type != EventType.SelectionChanged || editor == null)
                return false;

            int caretIndex = (int)args;

            return !editor.IsSameLine(caretIndex, lastIndex);
        }

        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        { }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        {
            var current = events.First();

            if (!IsTriggered(current.Type, current.EventArgs, view.Target))
                return;

            int caretIndex = (int)current.EventArgs;
            lastIndex = caretIndex;
        }
    }
}
