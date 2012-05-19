using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.Core.Common;
using FreePIE.Core.Model;
using FreePIE.GUI.Common.Visiblox;
using FreePIE.GUI.Events;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Curves
{
    public class CurveViewModel : PropertyChangedBase
    {
        private readonly IEventAggregator eventAggregator;
        public Curve Curve { get; private set; }

        public CurveViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public CurveViewModel Configure(Curve curve)
        {
            this.Curve = curve;
            SelectablePoints = curve.Points.Skip(1);
            Points = CalculateNewPoints();

            return this;
        }
        
        public string Name
        {
            get { return Curve.Name; }
            set 
            { 
                Curve.Name = value; 
                NotifyOfPropertyChange(() => Name);
            }
        }

        public void Delete()
        {
            eventAggregator.Publish(new DeleteCurveEvent(this));
        }

        public void OnPointDragged(object sender, MovePointBehaviour.PointMoveEventArgs e)
        {
            var oldPoint = e.OldPoint;
            var newPoint = e.NewPoint;

            var index = Curve.Points.FindIndex(p => p == e.OldPoint);
            var prevPoint = Curve.Points[index - 1];
            var biggestValueForY = double.MinValue;

            var newCurve = Curve.Points.GetRange(0, Curve.Points.Count);
            newCurve[index] = newPoint;
            var lastPoint = newCurve[newCurve.Count - 1];

            for (double x = 0; x < lastPoint.X; x++)
            {
                var y = CurveMath.SolveCubicSpline(newCurve, x);
                if (biggestValueForY > y)
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
        }

        private IEnumerable<Point> CalculateNewPoints()
        {
            var points = CurveMath.GetInterpolatedCubicSplinedCurve(Curve.Points);

            return points;
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
