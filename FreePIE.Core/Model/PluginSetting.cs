using System.Collections.Generic;

namespace FreePIE.Core.Model
{
    public class PluginSetting
    {
        public string FriendlyName { get; set; }
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