using System.Collections.Generic;

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
    }
}
