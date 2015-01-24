using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;

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
            view.PlacementRectangle = rect.IsEmpty ? default(Rect) : new Rect(CalculatePoint(rect, view), new Size(rect.Width, rect.Height));
        }

        Point CalculatePoint(Rect rect, CompletionPopupView view)
        {
            var offset = view.Target.GetOffset();
            return new Point(rect.X + offset.X, rect.Y + offset.Y + 1);
        }
    }
}
