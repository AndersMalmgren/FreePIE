using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class CloseOnWritingEndOfExpression : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        readonly Func<char, bool> isEndOfExpression;

        public CloseOnWritingEndOfExpression(Func<char, bool> isBeginningOfExpression)
        {
            isEndOfExpression = isBeginningOfExpression;
        }

        private bool IsTriggered(TextCompositionEventArgs args)
        {
            return args.Text.Length == 1 && isEndOfExpression(args.Text[0]);
        }

        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        {
            if (current.Type != EventType.CancellableInput || view.Target == null)
                return;

            if (!IsTriggered(current.EventArgs as TextCompositionEventArgs))
                return;

            PopupActions.Hide(view);
        }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        {

        }
    }
}