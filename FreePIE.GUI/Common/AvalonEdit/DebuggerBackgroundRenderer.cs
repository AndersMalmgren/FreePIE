using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace FreePIE.GUI.Common.AvalonEdit
{
    // Highlights lines with breakpoints and the line where the script is paused
    public class DebuggerBackgroundRenderer : IBackgroundRenderer
    {
        private static readonly Brush BreakpointBrush = Freeze(new SolidColorBrush(Color.FromArgb(0x50, 0xE5, 0x14, 0x00)));
        private static readonly Brush ExecutionLineBrush = Freeze(new SolidColorBrush(Color.FromArgb(0x90, 0xFF, 0xD8, 0x00)));

        private readonly BindableScriptEditor editor;

        public DebuggerBackgroundRenderer(BindableScriptEditor editor)
        {
            this.editor = editor;
        }

        private static T Freeze<T>(T freezable) where T : Freezable
        {
            freezable.Freeze();
            return freezable;
        }

        public KnownLayer Layer
        {
            get { return KnownLayer.Background; }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!textView.VisualLinesValid)
                return;

            foreach (var line in textView.VisualLines)
            {
                var lineNumber = line.FirstDocumentLine.LineNumber;
                var brush = lineNumber == editor.ExecutionLine ? ExecutionLineBrush : editor.HasBreakpoint(lineNumber) ? BreakpointBrush : null;

                if (brush != null)
                    drawingContext.DrawRectangle(brush, null, new Rect(0, line.VisualTop - textView.VerticalOffset, textView.ActualWidth, line.Height));
            }
        }
    }
}
