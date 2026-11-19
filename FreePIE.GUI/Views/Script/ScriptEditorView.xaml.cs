using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FreePIE.GUI.Common.AvalonEdit;

namespace FreePIE.GUI.Views.Script
{
    /// <summary>
    /// Interaction logic for ScriptEditorView.xaml
    /// </summary>
    public partial class ScriptEditorView
    {
        private SignatureHelpViewModel signatureHelp;
        private readonly ToolTip valueTip = new ToolTip { Placement = PlacementMode.Mouse };

        public ScriptEditorView()
        {
            InitializeComponent();

            var textArea = textEditor.TextArea;
            textArea.Caret.PositionChanged += (sender, args) => UpdateSignatureHelpPopup();
            textArea.TextView.ScrollOffsetChanged += (sender, args) => UpdateSignatureHelpPopup();
            textArea.IsKeyboardFocusWithinChanged += (sender, args) => UpdateSignatureHelpPopup();
            textArea.PreviewKeyDown += OnPreviewKeyDown;

            textEditor.MouseHover += OnMouseHover;
            textEditor.MouseHoverStopped += (sender, args) => valueTip.IsOpen = false;
            textEditor.BreakpointConditionRequested += OnBreakpointConditionRequested;

            DataContextChanged += OnDataContextChanged;
            Unloaded += (sender, args) =>
            {
                signatureHelpPopup.IsOpen = false;
                valueTip.IsOpen = false;
            };
        }

        // Shows the value of the expression under the mouse while the script is paused
        private void OnMouseHover(object sender, MouseEventArgs e)
        {
            var model = DataContext as ScriptEditorViewModel;
            if (model == null)
                return;

            var textView = textEditor.TextArea.TextView;
            var position = textView.GetPositionFloor(e.GetPosition(textView) + textView.ScrollOffset);
            if (!position.HasValue)
                return;

            var valueDescription = model.GetValueDescription(textEditor.Document.GetOffset(position.Value.Location));
            if (valueDescription == null)
                return;

            valueTip.PlacementTarget = textEditor;
            valueTip.Content = valueDescription;
            valueTip.IsOpen = true;
            e.Handled = true;
        }

        // The dialog is opened after the mouse event, a modal dialog inside the mouse handler would leave the margin with the mouse captured
        private void OnBreakpointConditionRequested(object sender, BreakpointConditionRequestedEventArgs e)
        {
            var model = DataContext as ScriptEditorViewModel;
            if (model != null)
                Dispatcher.BeginInvoke(new Action(() => Caliburn.Micro.Coroutine.BeginExecute(model.EditBreakpointCondition(e.LineNumber).GetEnumerator())));
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
