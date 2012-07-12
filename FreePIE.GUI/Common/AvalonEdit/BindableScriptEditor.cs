using System;
using System.Windows;
using System.Xml;
using FreePIE.GUI.Shells;
using FreePIE.GUI.Views.Script;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace FreePIE.GUI.Common.AvalonEdit
{
    public class BindableScriptEditor : TextEditor
    {
        static BindableScriptEditor()
        {
            // Load our custom highlighting definition
            IHighlightingDefinition customHighlighting;
            using (var s = typeof(MainShellView).Assembly.GetManifestResourceStream("FreePIE.GUI.Common.AvalonEdit.Lua.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (var reader = new XmlTextReader(s))
                {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting("Lua", new string[] { ".lua" }, customHighlighting);
        }

        public BindableScriptEditor()
        {
            TextArea.Caret.PositionChanged += CaretPositionChanged;
        }

        private void CaretPositionChanged(object sender, EventArgs e)
        {
            Caret = CaretOffset;
        }

        public ScriptEditorViewModel ViewModel
        {
            get { return DataContext as ScriptEditorViewModel; }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            Script = Text;
        }

        public static readonly DependencyProperty CaretProperty =
            DependencyProperty.Register("Caret", typeof (int), typeof (BindableScriptEditor), new PropertyMetadata(default(int), OnCaretPositionChanged));

        private static void OnCaretPositionChanged(object sender, DependencyPropertyChangedEventArgs caretChangedEventArgs)
        {
            var editor = sender as BindableScriptEditor;
            if (editor.CaretOffset != (int)caretChangedEventArgs.NewValue)
                editor.CaretOffset = (int) caretChangedEventArgs.NewValue;
        }

        public int Caret
        {
            get { return (int) GetValue(CaretProperty); }
            set { SetValue(CaretProperty, value); }
        }

        public string Script
        {
            get { return this.GetValue(ScriptProperty) as string; }
            set { this.SetValue(ScriptProperty, value); }
        }

        public static void OnScriptChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = sender as BindableScriptEditor;
            if(editor.Text != e.NewValue)
                editor.Text = e.NewValue as string;
        }
        
        public static readonly DependencyProperty ScriptProperty = DependencyProperty.Register(
          "Script", typeof(String), typeof(BindableScriptEditor), new FrameworkPropertyMetadata(string.Empty, OnScriptChanged));
    }
}
