using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using FreePIE.Core.Common.Extensions;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Persistence.Paths;

namespace FreePIE.Core.Persistence
{
    internal class SettingsManager : ISettingsManager
    {
        private readonly IPaths paths;
        private const string filename = "settings.xml";

        public SettingsManager(IPaths paths)
        {
            this.paths = paths;
        }

        public void Load()
        {
            var path = paths.GetDataPath(filename);

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

            FixBackwardCompatibility();
        }

        private void FixBackwardCompatibility()
        {
            Settings.Curves.Where(c => !c.ValidateCurve.HasValue).ForEach(c => c.ValidateCurve = true);
        }

        public void Save()
        {
            var serializer = new DataContractSerializer(typeof(Settings));
            using (var stream = new FileStream(paths.GetDataPath(filename), FileMode.Create))
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
            return Settings
                .PluginSettings
                .Where(ps => ps.PluginProperties.Any())
                .OrderBy(ps => ps.FriendlyName)
                .ToList();
        }

        public IEnumerable<PluginSetting> ListPluginSettingsWithHelpFile()
        {
            return Settings
                .PluginSettings
                .Where(ps => !string.IsNullOrEmpty(ps.HelpFile))
                .OrderBy(ps => ps.FriendlyName)
                .ToList();
        }

        public Settings Settings { get; private set; }

    }
}
