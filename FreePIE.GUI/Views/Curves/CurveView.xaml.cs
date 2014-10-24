using FreePIE.GUI.Common.Visiblox;

namespace FreePIE.GUI.Views.Curves
{
    /// <summary>
    /// Interaction logic for CurveView.xaml
    /// </summary>
    public partial class CurveView
    {
        public CurveView()
        {
            InitializeComponent();
        }

        private void OnPointDragged(object sender, MovePointBehaviour.PointMoveEventArgs e)
        {
            (DataContext as CurveViewModel).OnPointDragged(e);
        }

        private void OnPointSelected(object sender, MovePointBehaviour.PointSelectedEventArgs e)
        {
            (DataContext as CurveViewModel).OnPointSelected(e);
        }
    }
}
