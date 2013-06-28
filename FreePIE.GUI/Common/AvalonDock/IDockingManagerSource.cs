using Xceed.Wpf.AvalonDock;

namespace FreePIE.GUI.Common.AvalonDock
{
    internal interface IDockingManagerSource
    {
        DockingManager DockingManager { get; }
    }
}