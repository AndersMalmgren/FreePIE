using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FreePIE.GUI.Shells
{
    // The condition is a single expression, Enter accepts the dialog instead of adding a line
    public partial class BreakpointConditionView
    {
        public BreakpointConditionView()
        {
            InitializeComponent();

            PreviewKeyDown += OnPreviewKeyDown;
            DataObject.AddPastingHandler(conditionEditor.TextArea, OnPasting);

            Loaded += (sender, args) =>
            {
                conditionEditor.TextArea.Focus();
                conditionEditor.CaretOffset = conditionEditor.Document.TextLength;
            };
        }

        // The completion popup handles Enter and Escape itself while it's open, they must not reach the dialog
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && completionPopup.IsOpen)
            {
                completionPopup.IsOpen = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && !completionPopup.IsOpen)
            {
                Ok.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                e.Handled = true;
            }
        }

        private static void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            var text = e.DataObject.GetData(DataFormats.UnicodeText) as string;
            if (text == null || text.IndexOf('\n') < 0)
                return;

            var singleLine = new DataObject();
            singleLine.SetText(Regex.Replace(text.Trim(), @"\s*\r?\n\s*", " "));
            e.DataObject = singleLine;
        }
    }
}
