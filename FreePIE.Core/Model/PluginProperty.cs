using FreePIE.Core.Contracts;

namespace FreePIE.Core.Model
{
    public class PluginProperty : IPluginProperty
    {
        public object DefaultValue { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
    }
}