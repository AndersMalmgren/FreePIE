using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.Core.Model;
using FreePIE.GUI.Result;

namespace FreePIE.GUI.Shells.Curves
{
    public class NewCurveViewModel : ShellPresentationModel
    {
        private double min;
        private double max;
        private int pointCount;

        public NewCurveViewModel(IResultFactory resultFactory) : base(resultFactory)
        {
            Min = 0;
            Max = 180;
            PointCount = 7;

            DisplayName = "Curve initial data";
        }

        public void Init(Curve fromCurve)
        {
            Name = fromCurve.Name;
            Min = fromCurve.Points.First().X;
            Max = fromCurve.Points.Skip(fromCurve.Points.Count - 1).First().X;
            PointCount = fromCurve.Points.Count();
        }

        public string Name { get; set; }

        public double Min
        {
            get { return min; }
            set
            {
                min = value;
                NotifyOfPropertyChange(() => Min);
            }
        }

        public double Max
        {
            get { return max; }
            set
            {
                max = value;
                NotifyOfPropertyChange(() => Max);
            }
        }

        public int PointCount
        {
            get { return pointCount; }
            set
            {
                pointCount = value;
                NotifyOfPropertyChange(() => PointCount);
            }
        }

        public Curve NewCurve { get; private set; }

        public IEnumerable<IResult> Ok()
        {
            if (pointCount < 2)
                pointCount = 2;

            NewCurve = Curve.Create(Name, Min, Max, PointCount);
            yield return Result.Close();
        }
    }
}
