using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Model
{
    public class Settings
    {
        public List<Curve> Curves { get; set; }
        public List<PluginSetting> PluginSettings { get; set; }
        public bool MinimizeToTray { get; set; }

        public List<string> RecentScripts { get; set; }

        public Settings()
        {
            PluginSettings = new List<PluginSetting>();
            Curves = new List<Curve>();
            RecentScripts = new List<string>();
        }

        public void AddPluginSetting(PluginSetting pluginSetting)
        {
            PluginSettings.Add(pluginSetting);
        }

        public void AddNewCurve(Curve curve)
        {
            Curves.Add(curve);
            Curves = new List<Curve>(Curves.OrderBy(c => c.Name));
        }

        public void RemoveCurve(Curve curve)
        {
            Curves.Remove(curve);
        }

        public void AddRecentScript(string path)
        {
            if (path != null)
            {
                const int n = 10;
                RecentScripts.Remove(path);
                RecentScripts.Insert(0,path);
                if(RecentScripts.Count > n) 
                    RecentScripts.RemoveRange(n, RecentScripts.Count-n);
            }
        }
    }
}
