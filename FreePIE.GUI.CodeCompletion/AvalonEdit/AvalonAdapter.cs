using System;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Editing;

namespace FreePIE.GUI.CodeCompletion.AvalonEdit
{
    public class AvalonEditorAdapter : EditorAdapterBase
    {
        private readonly TextArea textArea;

        public AvalonEditorAdapter(TextArea textArea)
        {
            this.textArea = textArea;
            textArea.Caret.PositionChanged += OnCaretPositionChanged;
            textArea.PreviewKeyDown += OnTextAreaPreviewKeyDown;
            textArea.KeyDown += OnKeyDown;
            textArea.KeyUp += OnKeyUp;
            textArea.PreviewTextInput += OnPreviewTextInput;
        }

        void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (PreviewTextInput != null)
                PreviewTextInput(sender, e);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (KeyDown != null)
                KeyDown(sender, e);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (KeyUp != null)
                KeyUp(sender, e);
        }

        private void OnTextAreaPreviewKeyDown(object sender, KeyEventArgs e)
        {
            OnPreviewKeyDown(sender, e);
        }

        protected virtual void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (PreviewKeyDown != null)
                PreviewKeyDown(sender, e);
        }

        private void OnCaretPositionChanged(object sender, EventArgs e)
        {
            OnSelectionChanged(sender, e);
        }

        protected virtual void OnSelectionChanged(object sender, EventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(sender, e);
        }

        public override event EventHandler SelectionChanged;
        public override event KeyEventHandler PreviewKeyDown;
        public override event KeyEventHandler KeyDown;
        public override event KeyEventHandler KeyUp;
        public override event TextCompositionEventHandler PreviewTextInput;

        public override Rect GetVisualPosition()
        {
            return textArea.Caret.CalculateCaretRectangle();
        }

        public override UIElement UIElement
        {
            get { return textArea; }
        }

        public override string Text
        {
            get { return textArea.Document.Text; }
        }

        public override int CaretIndex
        {
            get { return textArea.Caret.Offset; }
        }

        public override bool IsSameLine(int charIndex1, int charIndex2)
        {
            int size = textArea.Document.TextLength;


            //Clamp to within text length to prevent out of bounds errors.
            charIndex1 = Math.Min(charIndex1, size);
            charIndex2 = Math.Min(charIndex2, size);

            return textArea.Document.GetLineByOffset(charIndex1) == textArea.Document.GetLineByOffset(charIndex2);
        }

        public override void Focus()
        {
            textArea.Focus();
        }
    }
}
