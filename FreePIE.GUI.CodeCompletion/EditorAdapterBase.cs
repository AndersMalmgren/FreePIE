using System;
using System.Windows;
using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion
{
    public abstract class EditorAdapterBase : DependencyObject
    {
        public abstract event EventHandler SelectionChanged;
        public abstract event KeyEventHandler PreviewKeyDown;
        public abstract Rect GetVisualPosition();
        public abstract int CaretIndex { get; }
        public abstract string Text { get; }
        public abstract UIElement UIElement { get; }
        public abstract bool IsSameLine(int charIndex1, int charIndex2);
        public abstract void Focus();
        public abstract event KeyEventHandler KeyDown;
        public abstract event KeyEventHandler KeyUp;
        public abstract event TextCompositionEventHandler PreviewTextInput;
    }
}
