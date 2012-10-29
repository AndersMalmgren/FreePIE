using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using FreePIE.GUI.CodeCompletion.Event.Events;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class OpenOnWriteAction : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        private Func<char, bool> isBeginningOfExpression;
        
        public OpenOnWriteAction(Func<char, bool> isBeginningOfExpression)
        {
            this.isBeginningOfExpression = isBeginningOfExpression;
        }

        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        {
            if (current.Type != EventType.CancellableInput || view.IsOpen)
                return;

            var args = current.EventArgs as TextCompositionEventArgs;

            if(args.Text.Length == 1 && isBeginningOfExpression(args.Text[0]))
                PopupActions.ForceShow(view);
        }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        {

        }
    }
}
