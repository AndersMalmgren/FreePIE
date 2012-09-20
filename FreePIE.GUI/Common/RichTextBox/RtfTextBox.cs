using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace FreePIE.GUI.Common.RichTextBox
{
    public class RtfTextBox : System.Windows.Controls.RichTextBox
    {
        public RtfTextBox()
        {
            IsReadOnly = true;
            IsDocumentEnabled = true;
            AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(HyperLinkClick));
        }

        private void HyperLinkClick(object sender, RoutedEventArgs e)
        {
            var link = e.OriginalSource as Hyperlink;
            if (link != null)
            {
                Process.Start(link.NavigateUri.ToString());
            }
        }

        public Stream RtfDocument
        {
            get { return this.GetValue(RtfDocumenProperty) as Stream; }
            set { this.SetValue(RtfDocumenProperty, value); }
        }

        public static void OnRtfDocumentChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as RtfTextBox;
            if (e.NewValue != null && e.NewValue != e.OldValue)
                textBox.Selection.Load(e.NewValue as Stream, DataFormats.Rtf);
        }

        public static readonly DependencyProperty RtfDocumenProperty = DependencyProperty.Register(
          "RtfDocument", typeof(Stream), typeof(RtfTextBox), new FrameworkPropertyMetadata(null, OnRtfDocumentChanged));
    }
}
