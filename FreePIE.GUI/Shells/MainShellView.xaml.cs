using FreePIE.GUI.Common.AvalonDock;
using Xceed.Wpf.AvalonDock;

namespace FreePIE.GUI.Shells
{
    /// <summary>
    /// Interaction logic for MainShell.xaml
    /// </summary>
    public partial class MainShellView : IDockingManagerSource
    {
        public MainShellView()
        {
            InitializeComponent();

         
        }

        public DockingManager DockingManager { get { return Manager; } }
    }
}
