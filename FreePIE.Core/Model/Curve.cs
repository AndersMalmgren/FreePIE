using System.Collections.Generic;

namespace FreePIE.Core.Model
{
    public struct Curve
    {
        public Curve(IEnumerable<Point> points) : this()
        {
            Points = points;
        }

        public IEnumerable<Point> Points { get; set; }
        public string Name { get; set; }
    }

    public struct Point
    {
        public Point(double x, double y) : this()
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }
    }
}
