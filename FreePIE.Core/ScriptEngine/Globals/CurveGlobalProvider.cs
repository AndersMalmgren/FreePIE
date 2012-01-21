using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Common;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Persistence;

namespace FreePIE.Core.ScriptEngine.Globals
{
    public class CurveGlobalProvider : IGlobalProvider
    {
        private readonly ISettingsManager settingsManager;
        public CurveGlobalProvider(ISettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
        }

        public IEnumerable<object> ListGlobals()
        {
            return settingsManager.Settings.Curves.Select(c => new CurveGlobal(c));
        }

        private class CurveGlobal : IGlobalNameProvider
        {
            private readonly Curve curve;
            private readonly List<Point> points;

            public CurveGlobal(Curve curve)
            {
                points = new List<Point>(curve.Points);
                this.curve = curve;
            }

            public double getY(double x)
            {
                return CurveMath.SolveCubicSpline(points, x);
            }

            public string Name { get { return curve.Name; } }
        }
    }
}
