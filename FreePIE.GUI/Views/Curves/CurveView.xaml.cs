using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FreePIE.GUI.Common.Visiblox;

namespace FreePIE.GUI.Views.Curves
{
    /// <summary>
    /// Interaction logic for CurveView.xaml
    /// </summary>
    public partial class CurveView : UserControl
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
