using FreePIE.Core.Contracts;
using FreePIE.Core.Model;

using System;
using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [Global(Name = "curves")]
    public class CurveHelper : IScriptHelper
    {
        public CurveHelper()
        {

        }

        /// <summary>
        /// Create a curve from a list of points
        /// </summary>
        /// <param name="points">list of points in the format x,y,x,y,x,y,x,y...</param>
        /// <returns>a curve global</returns>
        public CurveGlobalProvider.CurveGlobal create(double minimum, double maximum, params double[] points)
        {

            var pointz = new List<Point>() { new Point(minimum, minimum) };

            // ensure that all of the points values are between the minimum and maximum

            if (points.Any(p => p < minimum || p > maximum))
                throw new Exception("All points must be between the minimum and maximum values");


            pointz.AddRange(points.Select((x, i) => new { x, i }).GroupBy(p => p.i / 2).Select(g => new Point(g.First().x, g.Last().x)));

            pointz.Add(new Point(maximum, maximum));

            return new CurveGlobalProvider.CurveGlobal(new Curve(Guid.NewGuid().ToString(), pointz) { ValidateCurve = true });
        }

    }
   
}
