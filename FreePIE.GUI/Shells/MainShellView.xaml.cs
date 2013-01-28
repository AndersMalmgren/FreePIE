using System;
using System.Windows;
using System.Xml;
using AvalonDock;
using FreePIE.GUI.Common.AvalonDock;
using ICSharpCode.AvalonEdit.Highlighting;

namespace FreePIE.GUI.Shells
{
    /// <summary>
    /// Interaction logic for MainShell.xaml
    /// </summary>
    public partial class MainShellView : Window, IDockingManagerSource
    {
        public MainShellView()
        {
            InitializeComponent();

         
        }

        public DockingManager DockingManager { get { return Manager; } }
    }
}
