using System;
using System.Windows;
using System.Xml;
using FreePIE.GUI.Shells;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace FreePIE.GUI.Common.AvalonEdit
{
    public class BindableScriptEditor : TextEditor
    {
        static BindableScriptEditor()
        {
            LoadHighlightingFromManifest("FreePIE.GUI.Common.AvalonEdit.Python.xshd", "Python", ".py");
        }

        private static void LoadHighlightingFromManifest(string resource, string languageName, string fileExtension)
        {
            // Load our custom highlighting definition
            IHighlightingDefinition customHighlighting;
            using (var s = typeof(MainShellView).Assembly.GetManifestResourceStream(resource))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (var reader = new XmlTextReader(s))
                {
                    customHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting(languageName, new[] { fileExtension }, customHighlighting);
        }

        public BindableScriptEditor()
        {
            TextArea.Caret.PositionChanged += CaretPositionChanged;
        }
        
        private void CaretPositionChanged(object sender, EventArgs e)
        {
            Caret = CaretOffset;
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

        public Replacer Replacer
        {
            get { return this.GetValue(ReplacerProperty) as Replacer; }
            set { this.SetValue(ReplacerProperty, value); }            
        }

        public static void OnReplacerChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = sender as BindableScriptEditor;
            var replacer = e.NewValue as Replacer;
            replacer.Replace = editor.Document.Replace;
        }

        public static void OnScriptChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = sender as BindableScriptEditor;
            var newValue = e.NewValue as string;
            if (editor.Text != newValue)
                editor.Text = newValue;
        }

        public static readonly DependencyProperty ScriptProperty = DependencyProperty.Register(
          "Script", typeof(String), typeof(BindableScriptEditor), new FrameworkPropertyMetadata(string.Empty, OnScriptChanged));

        public static readonly DependencyProperty ReplacerProperty = DependencyProperty.Register(
            "Replacer", typeof(Replacer), typeof(BindableScriptEditor), new FrameworkPropertyMetadata(null, OnReplacerChanged));
    }

    public class Replacer
    {
        public Action<int, int, string> Replace { get; set; }
    }
}
