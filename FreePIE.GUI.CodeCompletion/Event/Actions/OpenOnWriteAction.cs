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
        private Func<bool> isBeginningOfExpression;

        private static readonly IEnumerable<Key> triggers = Enumerable.Range(44, 26).Cast<Key>();
        private static readonly IEnumerable<Key> modifiers = new []{ Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt };

        public OpenOnWriteAction(Func<bool> isBeginningOfExpression)
        {
            this.isBeginningOfExpression = isBeginningOfExpression;
        }

        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        { }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        {
            if (events.First().Type != EventType.KeyPress || view.IsOpen || !isBeginningOfExpression())
                return;

            var keyArgs = events.First().EventArgs as KeyEventArgs;

            if(triggers.Contains(keyArgs.Key) && !modifiers.Any(keyArgs.KeyboardDevice.IsKeyDown))
                PopupActions.ForceShow(view);
        }
    }
}
