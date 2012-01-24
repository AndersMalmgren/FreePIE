using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ICSharpCode.AvalonEdit;

namespace FreePIE.GUI.AvalonEdit
{
    public class BindableScriptEditor : TextEditor
    {
        public BindableScriptEditor()
        {
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
