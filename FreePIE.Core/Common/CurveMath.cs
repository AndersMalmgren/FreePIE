using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Model;

namespace FreePIE.Core.Common
{
    public static class CurveMath
    {

        private const int precision = 20;

        public static List<Point> GetInterpolatedCubicSplinedCurve(IList<Point> points)
        {
            var output = new List<Point>();
            int np = points.Count; // number of points
            double[] yCoords = new double[np]; // Newton form coefficients
            double[] xCoords = new double[np]; // x-coordinates of nodes
            double y;
            double x;

            if (np > 0)
            {
                for (int i = 0; i < np; i++)
                {
                    var p = points[i];
                    xCoords[i] = p.X;
                    yCoords[i] = p.Y;
                }
                if (np > 1)
                {
                    double[] a = new double[np];
                    double x1;
                    double x2;
                    double[] h = new double[np];
                    for (int i = 1; i <= np - 1; i++)
                    {
                        h[i] = xCoords[i] - xCoords[i - 1];
                    }
                    if (np > 2)
                    {
                        double[] sub = new double[np - 1];
                        double[] diag = new double[np - 1];
                        double[] sup = new double[np - 1];

                        for (int i = 1; i <= np - 2; i++)
                        {
                            diag[i] = (h[i] + h[i + 1])/3;
                            sup[i] = h[i + 1]/6;
                            sub[i] = h[i]/6;
                            a[i] = (yCoords[i + 1] - yCoords[i])/h[i + 1] - (yCoords[i] - yCoords[i - 1])/h[i];
                        }
                        SolveTridiag(sub, diag, sup, ref a, np - 2);
                    }

                    output.Add(points.First());

                    for (int i = 1; i <= np - 1; i++)
                    {
                        // loop over intervals between nodes
                        for (int j = 1; j <= precision; j++)
                        {
                            x1 = (h[i]*j)/precision;
                            x2 = h[i] - x1;
                            y = ((-a[i - 1]/6*(x2 + h[i])*x1 + yCoords[i - 1])*x2 +
                                 (-a[i]/6*(x1 + h[i])*x2 + yCoords[i])*x1)/h[i];
                            x = xCoords[i - 1] + x1;

                            output.Add(new Point(x, y));
                        }
                    }
                }
            }

            return output;
        }

        public static double SolveCubicSpline(IList<Point> knownSamples, double z)
        {
            int np = knownSamples.Count;

            if (np > 1)
            {
                if (knownSamples[0].X == z) return knownSamples[0].Y; 

                double[] a = new double[np];

                double x1;

                double x2;

                double y;

                double[] h = new double[np];

                for (int i = 1; i <= np - 1; i++)
                {

                    h[i] = knownSamples[i].X - knownSamples[i - 1].X;

                }

                if (np > 2)
                {

                    double[] sub = new double[np - 1];

                    double[] diag = new double[np - 1];

                    double[] sup = new double[np - 1];

                    for (int i = 1; i <= np - 2; i++)
                    {

                        diag[i] = (h[i] + h[i + 1])/3;

                        sup[i] = h[i + 1]/6;

                        sub[i] = h[i]/6;

                        a[i] = (knownSamples[i + 1].Y - knownSamples[i].Y)/h[i + 1] -

                               (knownSamples[i].Y - knownSamples[i - 1].Y)/h[i];

                    }

                    // SolveTridiag is a support function, see Marco Roello's original code

                    // for more information at

                    // http://www.codeproject.com/useritems/SplineInterpolation.asp

                    SolveTridiag(sub, diag, sup, ref a, np - 2);

                }



                int gap = 0;

                double previous = double.MinValue;

                // At the end of this iteration, "gap" will contain the index of the interval

                // between two known values, which contains the unknown z, and "previous" will

                // contain the biggest z value among the known samples, left of the unknown z

                for (int i = 0; i < knownSamples.Count; i++)
                {

                    if (knownSamples[i].X < z && knownSamples[i].X > previous)
                    {

                        previous = knownSamples[i].X;

                        gap = i + 1;

                    }

                }

                x1 = z - previous;

                if (gap > h.Length - 1)
                    return z;

                x2 = h[gap] - x1;

                if (gap == 0)
                    return 0.0;

                y = ((-a[gap - 1]/6*(x2 + h[gap])*x1 + knownSamples[gap - 1].Y)*x2 +

                     (-a[gap]/6*(x1 + h[gap])*x2 + knownSamples[gap].Y)*x1)/h[gap];

                return y;

            }

            return 0;

        }

        private static void SolveTridiag(double[] sub, double[] diag, double[] sup, ref double[] b, int n)
        {
            /*                  solve linear system with tridiagonal n by n matrix a
                                using Gaussian elimination *without* pivoting
                                where   a(i,i-1) = sub[i]  for 2<=i<=n
                                        a(i,i)   = diag[i] for 1<=i<=n
                                        a(i,i+1) = sup[i]  for 1<=i<=n-1
                                (the values sub[1], sup[n] are ignored)
                                right hand side vector b[1:n] is overwritten with solution 
                                NOTE: 1...n is used in all arrays, 0 is unused */
            int i;
            /*                  factorization and forward substitution */
            for (i = 2; i <= n; i++)
            {
                sub[i] = sub[i]/diag[i - 1];
                diag[i] = diag[i] - sub[i]*sup[i - 1];
                b[i] = b[i] - sub[i]*b[i - 1];
            }
            b[n] = b[n]/diag[n];
            for (i = n - 1; i >= 1; i--)
            {
                b[i] = (b[i] - sup[i]*b[i + 1])/diag[i];
            }
        }
    }
}

