using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Extensions;
using FreePIE.Core.Model;
using FreePIE.Core.Model.Events;
using FreePIE.GUI.Common.Visiblox;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells.Curves;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;
using Point = FreePIE.Core.Model.Point;

namespace FreePIE.GUI.Views.Curves
{
    public class CurveViewModel : PropertyChangedBase
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IResultFactory resultFactory;
        public Curve Curve { get; private set; }
        private int? selectedPointIndex;

        public CurveViewModel(IEventAggregator eventAggregator, IResultFactory resultFactory)
        {
            this.eventAggregator = eventAggregator;
            this.resultFactory = resultFactory;
        }

        public CurveViewModel Configure(Curve curve)
        {
            Curve = curve;
            InitCurve();

            return this;
        }

        private void InitCurve()
        {
            SetSelectablePoints();
            Points = CalculateNewPoints();
        }

        private void SetSelectablePoints()
        {
            SelectablePoints = Curve.Points.Skip(1).TakeAllButLast();
        }

        public string Name
        {
            get { return Curve.Name; }
            set 
            { 
                Curve.Name = value;
                eventAggregator.Publish(new CurveChangedNameEvent(Curve));
                NotifyOfPropertyChange(() => Name);
            }
        }

        public bool ValidateCurve
        {
            get { return Curve.ValidateCurve.Value; }
            set
            {
                Curve.ValidateCurve = value; 
                NotifyOfPropertyChange(() => ValidateCurve);
            }
        }

        public IEnumerable<IResult> Delete()
        {
            var message = resultFactory.ShowMessageBox(string.Format("Delete {0}?", Curve.Name), "Curve will be deleted, continue?", MessageBoxButton.OKCancel);
            yield return message;

            if(message.Result == System.Windows.MessageBoxResult.OK)
                eventAggregator.Publish(new DeleteCurveEvent(this));
        }

        public IEnumerable<IResult> Reset()
        {
            var dialog = resultFactory.ShowDialog<NewCurveViewModel>().Configure(m => m.Init(Curve));
            yield return dialog;

            var newCurve = dialog.Model.NewCurve;
            if (newCurve != null)
            {
                var message = resultFactory.ShowMessageBox(string.Format("Reset {0}?", Curve.Name), "Curve will be reset, continue?", MessageBoxButton.OKCancel);
                yield return message;

                if (message.Result == System.Windows.MessageBoxResult.OK)
                {
                    Curve.Reset(newCurve);
                    InitCurve();
                    Name = newCurve.Name;
                    ValidateCurve = true;
                }
            }
        }

        public bool HasSelectedPoint
        {
            get { return selectedPointIndex.HasValue; }
        }

        public void ApplyNewValuesToSelectedPoint()
        {
            ApplyNewSelectedPoint(new Point(SelectedPointX, SelectedPointY));
        }


        public void OnPointSelected(MovePointBehaviour.PointSelectedEventArgs e)
        {
            var index = Curve.IndexOf(e.Point);

            selectedPointIndex = index;

            UpdateSelectedPoint();

            NotifyOfPropertyChange(() => HasSelectedPoint);
        }

        private void UpdateSelectedPoint()
        {
            var selected = GetSelectedPoint();
            SelectedPointX = selected.X;
            SelectedPointY = selected.Y;
        }

        private double selectedPointX;
        public double SelectedPointX
        {
            get { return selectedPointX; }
            set
            {
                selectedPointX = value;
                NotifyOfPropertyChange(() => SelectedPointX);
            }
        }

        private double selectedPointY;
        public double SelectedPointY
        {
            get { return selectedPointY; }
            set
            {
                selectedPointY = value;
                NotifyOfPropertyChange(() => SelectedPointY);
            }
        }

        private void ApplyNewSelectedPoint(Point newPoint)
        {
            var args = new MovePointBehaviour.PointMoveEventArgs
            {
                OldPoint = GetSelectedPoint(),
                NewPoint = newPoint
            };
            OnPointDragged(args);
            SetSelectablePoints();
        }

        private Point GetSelectedPoint()
        {
            if (selectedPointIndex.HasValue)
                return Curve.Points[selectedPointIndex.Value];

            return new Point();
        }

        public void OnPointDragged(MovePointBehaviour.PointMoveEventArgs e)
        {
            var oldPoint = e.OldPoint;
            var newPoint = e.NewPoint;
            
            var index = Curve.IndexOf(e.OldPoint);
            
            var newCurve = Curve.Points.GetRange(0, Curve.Points.Count);
            newCurve[index] = newPoint;

            var firstPoint = newCurve[0];
            var lastPoint = newCurve[newCurve.Count - 1];

            var biggestValueForY = double.MinValue;

            if(ValidateCurve)
                for (double x = firstPoint.X + 0.01; x < lastPoint.X - 0.01; x++)
                {
                    var y = CurveMath.SolveCubicSpline(newCurve, x);
                    if (y < biggestValueForY || newPoint.X >= lastPoint.X || newPoint.X <= firstPoint.X)
                    {
                        newPoint = oldPoint;
                        break;
                    }
                    
                    if (y > biggestValueForY)
                        biggestValueForY = y;
                }

            e.NewPoint = newPoint;
            Curve.Points[index] = e.NewPoint;

            Points = CalculateNewPoints();
            UpdateSelectedPoint();
        }

        private IEnumerable<Point> CalculateNewPoints()
        {
            return CurveMath.GetInterpolatedCubicSplinedCurve(Curve.Points);
        }

        private IEnumerable<Point> points;
        public IEnumerable<Point> Points
        {
            get { return points; }
            set
            {
                points = value;
                NotifyOfPropertyChange(() => Points);
            }
        }

        private IEnumerable<Point> selectablePoints;

        public IEnumerable<Point> SelectablePoints
        {
            get { return selectablePoints; }
            set
            {
                selectablePoints = value;
                NotifyOfPropertyChange(() => SelectablePoints);
            }
        }

    }
}
