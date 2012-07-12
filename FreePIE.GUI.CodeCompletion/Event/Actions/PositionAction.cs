using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FreePIE.GUI.CodeCompletion.Event.Events;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public class PositionAction : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        { }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        {
            if (!events.Any())
                return;

            var last = events.First();

            if (last.Type != EventType.PositionInvalidated && last.Type != EventType.SelectionChanged && last.Type != EventType.PopupStateChanged)
                return;

            if (last.Type == EventType.SelectionChanged && view.IsOpen)
                return;

            Rect rect = view.Target.GetVisualPosition();
            view.PlacementRectangle = rect.IsEmpty ? default(Rect) : new Rect(rect.X, rect.Y + 1, rect.Width, rect.Height);
        }
    }
}
