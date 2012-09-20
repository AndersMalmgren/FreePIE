using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.Core.Model;

namespace FreePIE.GUI.Views.Plugin
{
    public class PluginHelpFileViewModel : PropertyChangedBase
    {
        public PluginSetting PluginSetting { get; private set; }

        public string Name { get { return PluginSetting.FriendlyName; } }
        public string HelpFile { get { return PluginSetting.HelpFile; } }

        public PluginHelpFileViewModel(PluginSetting pluginSetting)
        {
            PluginSetting = pluginSetting;
        }
    }
}
