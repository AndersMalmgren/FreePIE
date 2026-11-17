using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            return settingsManager.Settings.Curves.Where(c => !string.IsNullOrEmpty(c.Name)).Select(c => new CurveGlobal(c));
        }

        [DebuggerDisplay("{curve.Points}")]
        [Documentation("A curve, interpolated with a cubic spline through its points")]
        public class CurveGlobal : IGlobalNameProvider
        {
            private readonly Curve curve;

            public CurveGlobal(Curve curve)
            {
                this.curve = curve;
            }

            [Documentation("Returns the y value of the curve at x")]
            public double getY(double x)
            {
                return CurveMath.SolveCubicSpline(curve.Points, x);
            }

            [Documentation("The name of the curve")]
            public string Name { get { return curve.Name; } }
        }
    }
}
