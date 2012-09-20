using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using FreePIE.Core.Common;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;

namespace FreePIE.Core.Persistence
{
    internal class SettingsManager : ISettingsManager
    {
        private const string filename = "settings.xml";

        public void Load()
        {
            var path = Utils.GetAbsolutePath(filename);

            if (!File.Exists(path))
            {
                Settings = new Settings();
            }
            else
            {
                var serializer = new DataContractSerializer(typeof (Settings));
                using (var stream = new FileStream(path, FileMode.Open))
                {

                    Settings = serializer.ReadObject(stream) as Settings;
                }
            }
        }

        public void Save()
        {
            var serializer = new DataContractSerializer(typeof(Settings));
            using (var stream = new FileStream(Utils.GetAbsolutePath(filename), FileMode.Create))
            {
                serializer.WriteObject(stream, Settings);
            }
        }

        public PluginSetting GetPluginSettings(IPlugin plugin)
        {
            var pluginTypeName = plugin.GetType().FullName;
            var pluginSetting = Settings.PluginSettings.Single(ps => ps.PluginType == pluginTypeName);
            return pluginSetting;
        }

        public IEnumerable<PluginSetting> ListConfigurablePluginSettings()
        {
            return Settings.PluginSettings.Where(ps => ps.PluginProperties.Any()).ToList();
        }

        public IEnumerable<PluginSetting> ListPluginSettingsWithHelpFile()
        {
            return Settings.PluginSettings.Where(ps => !string.IsNullOrEmpty(ps.HelpFile)).ToList();
        }

        public Settings Settings { get; private set; }

    }
}
