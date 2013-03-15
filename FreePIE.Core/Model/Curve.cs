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
        public int IndexOf(Point point)
        {
            return Points.FindIndex(p => p == point);
        }

        public void Reset(double y)
        {
            Points = CalculateDefault(y);
        }

        public static Curve Create()
        {
            return new Curve(CalculateDefault(180));
        }

        private static List<Point> CalculateDefault(double y)
        {
            const int pointCount = 6;
            var points = new List<Point>();

            var step = y / (pointCount - 1);
            for (int i = 0; i < pointCount; i++)
            {
                var point = new Point(i * step, i * step);
                points.Add(point);
            }

            return points;
        }
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
