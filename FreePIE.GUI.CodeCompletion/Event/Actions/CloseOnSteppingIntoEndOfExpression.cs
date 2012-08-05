using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class CloseOnSteppingIntoEndOfExpression : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        readonly Func<bool> isEndOfExpression;

        public CloseOnSteppingIntoEndOfExpression(Func<bool> isBeginningOfExpression)
        {
            this.isEndOfExpression = isBeginningOfExpression;
        }

        private bool IsTriggered(KeyEventArgs key)
        {
            return isEndOfExpression() && key.Key == Key.Left;
        }

        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        { }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        {
            var current = events.First();

            if (current.Type != EventType.KeyUp || view.Target == null)
                return;

            if (!IsTriggered(current.EventArgs as KeyEventArgs))
                return;

            PopupActions.Hide(view);
        }
    }
}
