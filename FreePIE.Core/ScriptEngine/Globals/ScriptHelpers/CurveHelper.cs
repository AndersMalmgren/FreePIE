using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [Global(Name = "curves")]
    [Documentation("Creates curves from code. Curves made in the curve settings are globals named after the curve")]
    public class CurveHelper : IScriptHelper
    {
        [Documentation("Creates a curve from a list of (x, y) points sorted on x, for example curves.create([(0, 0), (0.5, 0.2), (1, 1)])")]
        public CurveGlobalProvider.CurveGlobal create(IEnumerable points)
        {
            if (points == null)
                throw new ArgumentException("Points must be a list of (x, y) pairs");

            var curvePoints = points.Cast<object>().Select(ToPoint).ToList();

            if (curvePoints.Count < 2)
                throw new ArgumentException("A curve needs at least two points");

            for (var i = 1; i < curvePoints.Count; i++)
                if (curvePoints[i].X <= curvePoints[i - 1].X)
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Points must be sorted on x without duplicates, x = {0} comes after x = {1}", curvePoints[i].X, curvePoints[i - 1].X));

            return new CurveGlobalProvider.CurveGlobal(new Curve(curvePoints));
        }

        private static Point ToPoint(object item)
        {
            var pair = item as IList;
            if (pair == null || pair.Count != 2)
                throw new ArgumentException(string.Format("Each point must be a pair (x, y), got {0}", item));

            return new Point(ToDouble(pair[0]), ToDouble(pair[1]));
        }

        private static double ToDouble(object value)
        {
            try
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is InvalidCastException || e is FormatException)
            {
                throw new ArgumentException(string.Format("Point values must be numbers, got {0}", value));
            }
        }
    }
}
