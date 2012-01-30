using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Extensions;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Persistence;

namespace FreePIE.Core.Plugins
{
    public class PluginInvoker : IPluginInvoker
    {
        private readonly ISettingsManager settingsManager;
        private readonly Func<Type, IOPlugin> pluginFactory;
        private readonly IFileSystem fileSystem;
        private const string pluginFolder = "plugins";

        private IEnumerable<Type> pluginTypes;
        private IEnumerable<Type> globalEnumTypes; 

        public PluginInvoker(ISettingsManager settingsManager, Func<Type, IOPlugin> pluginFactory, IFileSystem fileSystem)
        {
            this.settingsManager = settingsManager;
            this.pluginFactory = pluginFactory;
            this.fileSystem = fileSystem;
        }

        public IEnumerable<Type> ListAllPluginTypes()
        {
            if (pluginTypes != null)
                return pluginTypes;

            var path = Utils.GetAbsolutePath(pluginFolder);
            var dlls = fileSystem.GetFiles(path, "*.dll");

            pluginTypes = dlls
                .Select(Assembly.LoadFile)
                .SelectMany(a => a.GetTypes().Where(t => typeof (IOPlugin).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)).ToList();

            return pluginTypes;
        }

        public IEnumerable<IOPlugin> InvokeAndConfigurePlugins(IEnumerable<Type> pluginTypes)
        {
            var plugins = pluginTypes.Select(t => pluginFactory(t)).ToList();
            plugins.ForEach(SetPluginProperties);
            return plugins;
        }

        public void PopulatePluginSettings()
        {
            var settings = settingsManager.Settings;

            var pluginTypes = ListAllPluginTypes();
            var removedPluginSettings = settings.PluginSettings.Where(ps => !pluginTypes.Any(pt => pt.FullName == ps.PluginType)).ToList();
            var addedPluginTypes = pluginTypes.Where(pt => !settings.PluginSettings.Any(ps => ps.PluginType == pt.FullName)).ToList();

            removedPluginSettings.ForEach(ps => settings.PluginSettings.Remove(ps));
            settings.PluginSettings.AddRange(addedPluginTypes.Select(ps => new PluginSetting(ps.FullName)));
            
            foreach(var pluginType in pluginTypes)
            {
                var plugin = pluginFactory(pluginType);
                var pluginSettings = settings.PluginSettings.First(ps => ps.PluginType == pluginType.FullName);

                pluginSettings.FriendlyName = plugin.FriendlyName;
                InitProperties(plugin, pluginSettings.PluginProperties);
            }
        }



        public IEnumerable<Type> ListAllGlobalEnumTypes()
        {
            if(globalEnumTypes != null)
                return globalEnumTypes;

            globalEnumTypes = ListAllPluginTypes().Select(t => t.Assembly).SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof (LuaGlobalEnum), false).Any())).Distinct().ToList();

            return globalEnumTypes;
        }

        private void InitProperties(IOPlugin plugin, List<PluginProperty> properties)
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

        private void SetPluginProperties(IOPlugin plugin)
        {
            var pluginSettings = settingsManager.GetPluginSettings(plugin);
            var properties = pluginSettings.PluginProperties;

            var props = new Dictionary<string, object>();

            properties.ForEach(p => props.Add(p.Name, p.Value));

            plugin.SetProperties(props);
        }


    }
}
