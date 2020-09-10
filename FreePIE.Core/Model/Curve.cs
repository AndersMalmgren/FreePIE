using System;
using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Model
{
    public class Curve
    {
        public Curve(List<Point> points) : this(null, points) {}

        public Curve(string name, List<Point> points)
        {
            Name = name;
            Points = points;
            ValidateCurve = true;
        }

        public Curve() {}

        public List<Point> Points { get; set; }
        public string Name { get; set; }
        public bool? ValidateCurve { get; set; }
        public int IndexOf(Point point)
        {
            return Points.FindIndex(p => p == point);
        }

        public void Reset(Curve newCurve)
        {
            Points = newCurve.Points;
        }

        public static Curve Create(string name, double yAxisMinValue, double yAxisMaxValue, int pointCount)
        {
            return new Curve(name, CalculateDefault(yAxisMinValue, yAxisMaxValue, pointCount));
        }

        private static List<Point> CalculateDefault(double yAxisMinValue, double yAxisMaxValue, int pointCount)
        {
            var deltaBetweenPoints = (yAxisMaxValue - yAxisMinValue) /(pointCount - 1);
            return Enumerable.Range(0, pointCount)
                       .Select(index => yAxisMinValue + (index * deltaBetweenPoints))
                      .Select(value => new Point(value, value))
                      .ToList();
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
            return Math.Abs(x.X - y.X) < double.Epsilon && Math.Abs(x.Y - y.Y) < double.Epsilon;
        }

        public static bool operator !=(Point x, Point y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Point && Equals((Point) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public bool Equals(Point other)
        {
            return this == other;
        }
    }
}
