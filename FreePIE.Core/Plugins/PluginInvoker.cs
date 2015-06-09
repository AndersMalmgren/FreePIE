using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FreePIE.Core.Common;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Persistence;
using FreePIE.Core.Persistence.Paths;

namespace FreePIE.Core.Plugins
{
    public class PluginInvoker : IPluginInvoker
    {
        private readonly ISettingsManager settingsManager;
	    private readonly IPluginDataSource dataSource;
	    private readonly IFactory<IPlugin> pluginFactory;
        private readonly IFileSystem fileSystem;
        private readonly IPaths paths;
        private const string helpFolder = "help";


        public PluginInvoker(ISettingsManager settingsManager, IPluginDataSource dataSource, IFactory<IPlugin> pluginFactory, IFileSystem fileSystem, IPaths paths)
        {
            this.settingsManager = settingsManager;
	        this.dataSource = dataSource;
	        this.pluginFactory = pluginFactory;
            this.fileSystem = fileSystem;
            this.paths = paths;
        }

        public IEnumerable<IPlugin> InvokeAndConfigurePlugins(IEnumerable<Type> pluginTypes)
        {
			var plugins = pluginTypes.Select(pluginFactory.Create).ToList();
            plugins.ForEach(SetPluginProperties);
            return plugins;
        }

        public void PopulatePluginSettings()
        {
            var settings = settingsManager.Settings;

			var pluginTypes = dataSource.ListAllPluginTypes();
            var removedPluginSettings = settings.PluginSettings.Where(ps => !pluginTypes.Any(pt => pt.FullName == ps.PluginType)).ToList();
            var addedPluginTypes = pluginTypes.Where(pt => !settings.PluginSettings.Any(ps => ps.PluginType == pt.FullName)).ToList();

            removedPluginSettings.ForEach(ps => settings.PluginSettings.Remove(ps));
            settings.PluginSettings.AddRange(addedPluginTypes.Select(ps => new PluginSetting(ps.FullName)));
            
            foreach(var pluginType in pluginTypes)
            {
                var plugin = pluginFactory.Create(pluginType);
                var pluginSettings = settings.PluginSettings.First(ps => ps.PluginType == pluginType.FullName);

                pluginSettings.FriendlyName = plugin.FriendlyName;
                InitProperties(plugin, pluginSettings.PluginProperties);

                var helpFile = string.Format(@"{0}\{1}.rtf", paths.GetApplicationPath(helpFolder), pluginType.FullName);

                if(fileSystem.Exists(helpFile))
                {
                    pluginSettings.HelpFile = helpFile;
                }
            }
        }

        private void InitProperties(IPlugin plugin, List<PluginProperty> properties)
        {
            int index = 0;
            bool moreProperties = false;
            do
            {
                if (index >= properties.Count)
                    properties.Add(new PluginProperty());

                moreProperties = plugin.GetProperty(index, properties[index]);
                if (moreProperties)
                {
                    var property = properties[index];
                    if (property.Value == null || (property.DefaultValue != null && property.Value.GetType() != property.DefaultValue.GetType()))
                        property.Value = property.DefaultValue;

                    index++;
                }
                else
                    properties.RemoveAt(index);
            }
            while (index < properties.Count || moreProperties);
        }

        private void SetPluginProperties(IPlugin plugin)
        {
            var pluginSettings = settingsManager.GetPluginSettings(plugin);
            var properties = pluginSettings.PluginProperties;

            plugin.SetProperties(properties.ToDictionary(p => p.Name, p => p.Value));
        }


    }
}
