using System.ComponentModel;
using System.Reflection;
using System.Windows;
using FreePIE.GUI.Common.AvalonDock;
using Xceed.Wpf.AvalonDock;

namespace FreePIE.GUI.Shells
{
    /// <summary>
    /// Interaction logic for MainShell.xaml
    /// </summary>
    public partial class MainShellView : IDockingManagerSource
    {
        private static readonly FieldInfo MenuDropAlignmentField;

        static MainShellView()
        {
            MenuDropAlignmentField = typeof (SystemParameters).GetField("_menuDropAlignment",
                BindingFlags.NonPublic | BindingFlags.Static);
            System.Diagnostics.Debug.Assert(MenuDropAlignmentField != null);

            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged += (s, e) => EnsureStandardPopupAlignment();
        }

        private static void EnsureStandardPopupAlignment()
        {
            if (SystemParameters.MenuDropAlignment && MenuDropAlignmentField != null)
            {
                MenuDropAlignmentField.SetValue(null, false);
            }
        }

        public MainShellView()
        {
            InitializeComponent();
        }

        public DockingManager DockingManager
        {
            get { return Manager; }
        }
    }
}


