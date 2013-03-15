using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class OpenOnWriteAction : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        private Func<char, bool> isBeginningOfExpression;

        private HashSet<char> triggers;
        
        public OpenOnWriteAction(Func<char, bool> isBeginningOfExpression)
        {
            triggers = new HashSet<char>(Enumerable.Range(65, 26).Union(Enumerable.Range(97, 26)).Select(x => (char)x))
            {
                '(',
                '.'
            };
            this.isBeginningOfExpression = isBeginningOfExpression;
        }

        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        {
            if (current.Type != EventType.CancellableInput || view.IsOpen)
                return;

            var args = current.EventArgs as TextCompositionEventArgs;

            if (args.Text.Length == 1 && triggers.Contains(args.Text[0]) && isBeginningOfExpression(args.Text[0]))
                PopupActions.Show(view);
        }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        {

        }
    }
}
