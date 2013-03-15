namespace FreePIE.GUI.Views.Script.Output
{
    /// <summary>
    /// Interaction logic for ConsoleView.xaml
    /// </summary>
    public partial class ConsoleView
    {
        public ConsoleView()
        {
            InitializeComponent();

            Text.TextChanged += (s, e) =>
                {
                    Text.CaretIndex = Text.Text.Length;
                    Text.ScrollToEnd(); 
                };
        }
    }
}
