using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FreePIE.Core.Model
{
    [DataContract]
    public class PluginSetting
    {
        public string FriendlyName { get; set; }
        [DataMember]
        public string PluginType { get; set; }
        [DataMember]
        public List<PluginProperty> PluginProperties { get; set; }
        public string HelpFile { get; set; }

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