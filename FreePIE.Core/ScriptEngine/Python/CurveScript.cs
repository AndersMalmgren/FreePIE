using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FreePIE.Core.Model;

namespace FreePIE.Core.ScriptEngine.Python
{
    public static class CurveScript
    {
        public static string Create(Curve curve)
        {
            var points = string.Join(", ", curve.Points.Select(p => string.Format(CultureInfo.InvariantCulture, "({0:0.###}, {1:0.###})", p.X, p.Y)));
            return string.Format("{0} = curves.create([{1}])", ToIdentifier(curve.Name), points);
        }

        private static string ToIdentifier(string name)
        {
            var identifier = Regex.Replace(name ?? string.Empty, "[^A-Za-z0-9_]", "_");

            if (identifier.Length == 0)
                return "curve";

            return char.IsDigit(identifier[0]) ? "_" + identifier : identifier;
        }
    }
}
