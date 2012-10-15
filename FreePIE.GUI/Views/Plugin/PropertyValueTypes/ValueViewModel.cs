using Caliburn.Micro;
using FreePIE.Core.Model;

namespace FreePIE.GUI.Views.Plugin.PropertyValueTypes
{
    public abstract class ValueViewModel : PropertyChangedBase
    {
        protected readonly PluginProperty pluginProperty;

        protected ValueViewModel(PluginProperty pluginProperty)
        {
            this.pluginProperty = pluginProperty;
        }

        public object Value
        {
            get { return pluginProperty.Value; }
            set { pluginProperty.Value = value; }
        }
    }
}
