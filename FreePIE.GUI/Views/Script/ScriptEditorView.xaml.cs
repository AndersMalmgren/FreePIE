using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace FreePIE.GUI.Views.Script
{
    /// <summary>
    /// Interaction logic for ScriptEditorView.xaml
    /// </summary>
    public partial class ScriptEditorView
    {
        private SignatureHelpViewModel signatureHelp;

        public ScriptEditorView()
        {
            InitializeComponent();

            var textArea = textEditor.TextArea;
            textArea.Caret.PositionChanged += (sender, args) => UpdateSignatureHelpPopup();
            textArea.TextView.ScrollOffsetChanged += (sender, args) => UpdateSignatureHelpPopup();
            textArea.IsKeyboardFocusWithinChanged += (sender, args) => UpdateSignatureHelpPopup();
            textArea.PreviewKeyDown += OnPreviewKeyDown;

            DataContextChanged += OnDataContextChanged;
            Unloaded += (sender, args) => signatureHelpPopup.IsOpen = false;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (signatureHelp != null)
                signatureHelp.PropertyChanged -= OnSignatureHelpChanged;

            var model = e.NewValue as ScriptEditorViewModel;
            signatureHelp = model != null ? model.SignatureHelp : null;

            if (signatureHelp != null)
                signatureHelp.PropertyChanged += OnSignatureHelpChanged;

            UpdateSignatureHelpPopup();
        }

        private void OnSignatureHelpChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateSignatureHelpPopup();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && signatureHelp != null && signatureHelpPopup.IsOpen)
                signatureHelp.Dismiss();
        }

        // The popup is placed above the caret and only shown while the editor has focus
        private void UpdateSignatureHelpPopup()
        {
            var textArea = textEditor.TextArea;
            var show = signatureHelp != null && signatureHelp.IsVisible && textArea.IsKeyboardFocusWithin;

            if (show)
            {
                var caret = textArea.Caret.CalculateCaretRectangle();
                var scroll = textArea.TextView.ScrollOffset;

                signatureHelpPopup.PlacementRectangle = new Rect(caret.X - scroll.X, caret.Y - scroll.Y, Math.Max(caret.Width, 1), caret.Height);
            }

            signatureHelpPopup.IsOpen = show;
        }
    }
}
