﻿using System.Collections.Generic;
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

        public void AddNewCurve(Curve curve)
        {
            Curves.Add(curve);
            Curves = new List<Curve>(Curves.OrderBy(c => c.Name));
        }

        public void RemoveCurve(Curve curve)
        {
            Curves.Remove(curve);
        }
    }
}
