using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
