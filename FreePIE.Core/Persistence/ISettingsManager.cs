using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;

namespace FreePIE.Core.Persistence
{
    public interface ISettingsManager
    {
        bool Load();
        void Save();
        Settings Settings { get; }
        PluginSetting GetPluginSettings(IPlugin plugin);
        IEnumerable<PluginSetting> ListConfigurablePluginSettings();
        IEnumerable<PluginSetting> ListPluginSettingsWithHelpFile();
    }
}