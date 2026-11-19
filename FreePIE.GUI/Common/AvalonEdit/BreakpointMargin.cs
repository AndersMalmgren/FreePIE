using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace FreePIE.GUI.Common.AvalonEdit
{
    // Shows breakpoints and the line where the script is paused, clicking a line toggles its breakpoint and right clicking edits its condition
    public class BreakpointMargin : AbstractMargin
    {
        private const double MarginWidth = 16;
        private const double BreakpointRadius = 5.5;
        private const double ConditionMarkSize = 3;

        private static readonly Brush BreakpointBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xE5, 0x14, 0x00)));
        private static readonly Pen ConditionPen = Freeze(new Pen(Brushes.White, 1.5));
        private static readonly Brush ExecutionLineBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xFF, 0xD8, 0x00)));
        private static readonly Pen ExecutionLinePen = Freeze(new Pen(Brushes.DimGray, 1));

        private readonly BindableScriptEditor editor;

        public BreakpointMargin(BindableScriptEditor editor)
        {
            this.editor = editor;
            Cursor = Cursors.Arrow;
        }

        private static T Freeze<T>(T freezable) where T : Freezable
        {
            freezable.Freeze();
            return freezable;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(MarginWidth, 0);
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
                oldTextView.VisualLinesChanged -= OnVisualLinesChanged;

            base.OnTextViewChanged(oldTextView, newTextView);

            if (newTextView != null)
                newTextView.VisualLinesChanged += OnVisualLinesChanged;

            InvalidateVisual();
        }

        private void OnVisualLinesChanged(object sender, System.EventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // A background makes the whole margin clickable
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            var textView = TextView;
            if (textView == null || !textView.VisualLinesValid)
                return;

            foreach (var line in textView.VisualLines)
            {
                var lineNumber = line.FirstDocumentLine.LineNumber;
                var center = new Point(MarginWidth / 2, line.VisualTop - textView.VerticalOffset + line.Height / 2);

                var breakpoint = editor.GetBreakpoint(lineNumber);
                if (breakpoint != null)
                {
                    drawingContext.DrawEllipse(BreakpointBrush, null, center, BreakpointRadius, BreakpointRadius);

                    if (breakpoint.HasCondition)
                    {
                        drawingContext.DrawLine(ConditionPen, new Point(center.X - ConditionMarkSize, center.Y), new Point(center.X + ConditionMarkSize, center.Y));
                        drawingContext.DrawLine(ConditionPen, new Point(center.X, center.Y - ConditionMarkSize), new Point(center.X, center.Y + ConditionMarkSize));
                    }
                }

                if (lineNumber == editor.ExecutionLine)
                    drawingContext.DrawGeometry(ExecutionLineBrush, ExecutionLinePen, CreateArrow(center));
            }
        }

        private static Geometry CreateArrow(Point center)
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(center.X - 6, center.Y - 2.5), true, true);
                context.LineTo(new Point(center.X, center.Y - 2.5), true, false);
                context.LineTo(new Point(center.X, center.Y - 6), true, false);
                context.LineTo(new Point(center.X + 6, center.Y), true, false);
                context.LineTo(new Point(center.X, center.Y + 6), true, false);
                context.LineTo(new Point(center.X, center.Y + 2.5), true, false);
                context.LineTo(new Point(center.X - 6, center.Y + 2.5), true, false);
            }

            geometry.Freeze();
            return geometry;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var lineNumber = GetLineNumber(e);
            if (lineNumber.HasValue)
                editor.ToggleBreakpoint(lineNumber.Value);

            e.Handled = true;
        }

        // Handled so the editor's context menu does not open
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            var lineNumber = GetLineNumber(e);
            if (lineNumber.HasValue)
                editor.RequestBreakpointCondition(lineNumber.Value);

            e.Handled = true;
        }

        private int? GetLineNumber(MouseEventArgs e)
        {
            var textView = TextView;
            if (textView == null || !textView.VisualLinesValid)
                return null;

            var line = textView.GetVisualLineFromVisualTop(e.GetPosition(textView).Y + textView.VerticalOffset);
            return line != null ? line.FirstDocumentLine.LineNumber : (int?)null;
        }
    }
}
