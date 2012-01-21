using System.Collections.Generic;

namespace FreePIE.Core.Model
{
    public class PluginSetting
    {
        public string PluginType { get; set; }
        public List<PluginProperty> PluginProperties { get; set; }

        public PluginSetting()
        {
            PluginProperties = new List<PluginProperty>();
        }
        public PluginSetting(string pluginType) : this()
        {
            PluginType = pluginType;
        }
    }
}