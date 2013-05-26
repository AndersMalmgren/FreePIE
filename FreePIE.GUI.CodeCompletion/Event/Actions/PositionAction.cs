using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            view.PlacementRectangle = rect.IsEmpty ? default(Rect) : new Rect(CalculatePoint(rect, view.Target.UIElement), new Size(rect.Width, rect.Height));
        }

        private ScrollViewer FindScrollAncestor(UIElement element)
        {
            DependencyObject obj = element;
            while((obj = LogicalTreeHelper.GetParent(obj)) != null)
            {
                if (obj is ScrollViewer)
                    return obj as ScrollViewer;
            }

            return null;
        }

        Point CalculatePoint(Rect rect, UIElement textArea)
        {
            var scroll = FindScrollAncestor(textArea);

            if(scroll == null)
                return new Point(rect.X, rect.Y + 1);

            return new Point(rect.X - scroll.HorizontalOffset, rect.Y - scroll.VerticalOffset + 1);
        }
    }
}
