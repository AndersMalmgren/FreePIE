using Caliburn.Micro;
using FreePIE.Core.Model;

namespace FreePIE.GUI.Views.Plugin.PropertyValueTypes
{
    public class ValueViewModel : PropertyChangedBase
    {
        protected readonly PluginProperty pluginProperty;

        public ValueViewModel(PluginProperty pluginProperty)
        {
            this.pluginProperty = pluginProperty;
        }
    }
}
