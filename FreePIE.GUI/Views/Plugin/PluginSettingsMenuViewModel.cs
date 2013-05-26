using Caliburn.Micro;
using FreePIE.Core.Model;

namespace FreePIE.GUI.Views.Plugin
{
    public class PluginSettingsMenuViewModel : PropertyChangedBase
    {
        public PluginSetting PluginSetting { get; private set; }
        public string Name { get { return PluginSetting.FriendlyName; } }

        public PluginSettingsMenuViewModel(PluginSetting pluginSetting)
        {
            PluginSetting = pluginSetting;
        }
    }
}
