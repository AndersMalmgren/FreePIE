using System;
using System.Windows;
using System.Windows.Input;
using Visiblox.Charts;

namespace FreePIE.GUI.Common.Visiblox
{
    /// <summary>
    /// This behaviour allows a user to alter the Y value of a point in a series by
    /// clicking on the point and moving up and down the chart. The new Y value gets
    /// committed only once the button is released.
    /// </summary>
    public class MovePointBehaviour : BehaviourBase
    {
        private bool _leftMouseDown;
        private BindableDataPoint _selectedPoint;
        private Cursor _currentCursor;
        
        private const double ALLOWED_POINT_DEVIATION = 10.0;
        
        public MovePointBehaviour() :
            base("MovePoint")
        {
        }

        public override void DeInit()
        {
            // Reset the cursor before we leave...
            Chart.Cursor = _currentCursor;
        }

        protected override void Init()
        { 
            // Store the current cursor in case we have to reset it during a point move.
            _currentCursor = Chart.Cursor;
        }

        /// <summary>
        /// Override of <see cref="BehaviourBase"/> that handles a left mouse button press.
        /// This initiates the point move process allowing a user to alter the Y value of
        /// the selected point.
        /// </summary>
        /// <param name="position">The position on the mouse on click.</param>
        public override void MouseLeftButtonDown(Point position)
        {


            // Get the point that has been clicked on.
            GetSelectedPoint(position);

            // Check we can capture the mouse, otherwise the move is impossible.
            bool captured = BehaviourContainer.CaptureMouse();
            if (captured && _selectedPoint != null)
            {
                _leftMouseDown = true;

                SetZoomEnabled(false);

                foreach(var child in this.BehaviourContainer.Children)
                {
                    if(child is ZoomBehaviour)
                    {
                        (child as ZoomBehaviour).IsEnabled = false;
                        break;
                    }
                }
                
                // Change the point style by adding it to the Series SelectedItems list
                //((ChartSingleSeriesBase)Chart.Series[0]).SelectedItems.Add(_selectedPoint);

                // Change the cursor
                _currentCursor = Chart.Cursor;
                Chart.Cursor = Cursors.Hand; 
            }
        }

        private void SetZoomEnabled(bool enabled)
        {
            var manager = Chart.Behaviour as BehaviourManager;
            if (manager != null)
            {
                foreach (var behaviour in manager.Behaviours)
                {
                    if (behaviour is ZoomBehaviour)
                    {
                        (behaviour as ZoomBehaviour).IsEnabled = enabled;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Override of <see cref="BehaviourBase"/> that handles the left mouse button being
        /// released. If the button was initially pressed on point, then this signifies the
        /// end of a move.
        /// </summary>
        /// <param name="position">The position the button was released at.</param>
        public override void MouseLeftButtonUp(Point position)
        {
            if (!_leftMouseDown)
            {
                return;
            }

            SetZoomEnabled(true);
            Chart.Cursor = _currentCursor;
            _leftMouseDown = false;
            _selectedPoint = null;
            BehaviourContainer.ReleaseMouseCapture();
        }

        /// <summary>
        /// Override of <see cref="BehaviourBase"/> that handles the mouse being dragged
        /// and changing the Y-value of the clicked point
        /// </summary>
        /// <param name="position">Position that the mouse moved to.</param>
        public override void MouseMove(Point position)
        {
            if (!_leftMouseDown || _selectedPoint == null)
            {
                return;
            }

            var x = Chart.XAxis.GetRenderPositionAsDataValueWithZoom(position.X);
            var y = Chart.YAxis.GetRenderPositionAsDataValueWithZoom(position.Y);

            var args = new PointMoveEventArgs { OldPoint = new Core.Model.Point((double)_selectedPoint.X, (double)_selectedPoint.Y), NewPoint = new Core.Model.Point((double)x, (double)y) };
            if (OnPointMove != null)
            {
                OnPointMove(this, args);

                _selectedPoint.XValue = args.NewPoint.X;
                _selectedPoint.YValue = args.NewPoint.Y;
            }

        }

        public event EventHandler<PointMoveEventArgs> OnPointMove;
        public class PointMoveEventArgs : EventArgs
        {
            public Core.Model.Point OldPoint { get; set; }
            public Core.Model.Point NewPoint { get; set; }
        }

        public event EventHandler<PointSelectedEventArgs> OnPointSelected;
        public class PointSelectedEventArgs : EventArgs
        {
            public Core.Model.Point Point { get; set; }
        }

        /// <summary>
        /// Handle the situation where we are moving a point (or have clicked on a point) and the
        /// behaviour becomes disabled.
        /// </summary>
        protected override void OnIsEnabledPropertyChanged()
        {
            if (_leftMouseDown && !IsEnabled)
            { 
                // Reset the cursor and remove the selected point from the series collection.
                Chart.Cursor = _currentCursor;
                _leftMouseDown = false;

                ((ChartSingleSeriesBase)Chart.Series[0]).SelectedItems.Remove(_selectedPoint);
                _selectedPoint = null;
            }
        }

        private void GetSelectedPoint(Point position)
        {
            // Get the point that is within range of the mouse click.
            foreach (BindableDataPoint dp in Chart.Series[0].DataSeries)
            {
                if (IsClickPositionCloseToPoint(position.X, Chart.XAxis.GetDataValueAsRenderPositionWithZoom(dp.X)) &&
                    IsClickPositionCloseToPoint(position.Y, Chart.YAxis.GetDataValueAsRenderPositionWithZoom(dp.Y)))
                {
                    _selectedPoint = dp;
                    var point = new Core.Model.Point((double)_selectedPoint.XValue, (double)_selectedPoint.XValue);
                    OnPointSelected(this, new PointSelectedEventArgs { Point = point});
                    break;
                }
            }
        }
        
        private bool IsClickPositionCloseToPoint(double positionVal, double pointVal)
        {
            return (((pointVal - ALLOWED_POINT_DEVIATION) < positionVal) &&
                    ((pointVal + ALLOWED_POINT_DEVIATION) > positionVal));
        }
    }
}
