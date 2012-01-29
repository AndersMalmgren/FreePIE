using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;

namespace FreePIE.Core.Persistence
{
    public interface ISettingsManager
    {
        void Load();
        void Save();
        Settings Settings { get; }
        PluginSetting GetPluginSettings(IOPlugin plugin);
        IEnumerable<PluginSetting> ListConfigurablePluginSettings();
    }
}