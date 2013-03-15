using System.Collections.Generic;
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

        public class CurveGlobal : IGlobalNameProvider
        {
            private readonly Curve curve;

            public CurveGlobal(Curve curve)
            {
                this.curve = curve;
            }

            public double getY(double x)
            {
                return CurveMath.SolveCubicSpline(curve.Points, x);
            }

            public string Name { get { return curve.Name; } }
        }
    }
}
