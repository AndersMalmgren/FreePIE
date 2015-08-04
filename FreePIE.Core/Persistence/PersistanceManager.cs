using FreePIE.Core.Plugins;

namespace FreePIE.Core.Persistence
{
    public class PersistanceManager : IPersistanceManager
    {
        private readonly ISettingsManager settingsManager;
        private readonly IPluginInvoker pluginInvoker;

        public PersistanceManager(ISettingsManager settingsManager, IPluginInvoker pluginInvoker)
        {
            this.settingsManager = settingsManager;
            this.pluginInvoker = pluginInvoker;
        }

        public bool Load()
        {
            var result = settingsManager.Load();
            pluginInvoker.PopulatePluginSettings();

            return result;
        }

        public void Save()
        {
            settingsManager.Save();
        }
    }
}
