using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using FreePIE.GUI.Shells;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace FreePIE.GUI.AvalonEdit
{
    public class BindableScriptEditor : TextEditor
    {
        static BindableScriptEditor()
        {
            // Load our custom highlighting definition
            IHighlightingDefinition customHighlighting;
            using (var s = typeof(MainShellView).Assembly.GetManifestResourceStream("FreePIE.GUI.AvalonEdit.Lua.xshd"))
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

        protected override void OnTextChanged(EventArgs e)
        {
            Script = Text;
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
