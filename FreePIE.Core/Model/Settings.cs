using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Model
{
    public class Settings
    {
        public List<Curve> Curves { get; set; }
        public List<PluginSetting> PluginSettings { get; set; }

        public Settings()
        {
            PluginSettings = new List<PluginSetting>();
            Curves = new List<Curve>();
        }

        public void AddPluginSetting(PluginSetting pluginSetting)
        {
            PluginSettings.Add(pluginSetting);
        }

        public void AddNewCurve()
        {
            
            int pointCount = 6;
            List<Point> points = new List<Point>();

            var step = 180 / (pointCount - 1);
            for (int i = 0; i < pointCount; i++)
            {
                var point = new Point(i * step, i * step);
                points.Add(point);
            }

            var curve = new Curve(points);

            Curves.Add(curve);
            Curves = new List<Curve>(Curves.OrderBy(c => c.Name));
        }

        public void RemoveCurve(Curve curve)
        {
            Curves.Remove(curve);
        }
    }
}
