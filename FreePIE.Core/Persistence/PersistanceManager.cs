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

        public void Load()
        {
            settingsManager.Load();
            pluginInvoker.PopulatePluginSettings();
        }

        public void Save()
        {
            settingsManager.Save();
        }
    }
}
