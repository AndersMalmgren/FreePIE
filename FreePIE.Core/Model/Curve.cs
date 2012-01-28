using System.Collections.Generic;

namespace FreePIE.Core.Model
{
    public class Curve
    {
        public Curve(List<Point> points)
        {
            Points = points;
        }

        public Curve() {}

        public List<Point> Points { get; set; }
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

        public static bool operator ==(Point x, Point y)
        {
            return x.X == y.X && y.Y == y.Y;
        }

        public static bool operator !=(Point x, Point y)
        {
            return !(x == y);
        }
    }
}
