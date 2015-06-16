using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FreePIE.Core.Common;
using FreePIE.Core.Contracts;
using FreePIE.Core.Persistence.Paths;

namespace FreePIE.Core.Plugins
{
    public class PluginDataSource : IPluginDataSource
    {
        private readonly IPaths paths;
        private readonly IFileSystem fileSystem;
        private IEnumerable<Type> pluginTypes;
        private IEnumerable<Type> globalEnumTypes;

        private const string pluginFolder = "plugins";

        public PluginDataSource(IPaths paths, IFileSystem fileSystem)
        {
            this.paths = paths;
            this.fileSystem = fileSystem;
        }

        public IEnumerable<Type> ListAllPluginTypes()
        {
            if(pluginTypes != null)
                return pluginTypes;

            var path = paths.GetApplicationPath(pluginFolder);
            var dlls = fileSystem.GetFiles(path, "*.dll");

            pluginTypes = dlls
                .Select(Assembly.LoadFile)
                .SelectMany(a => a.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)).ToList();

            return pluginTypes;
        }

        public IEnumerable<Type> ListAllGlobalEnumTypes()
        {
            if(globalEnumTypes != null)
                return globalEnumTypes;

            globalEnumTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttributes(typeof(GlobalEnum), false).Any())
                .Distinct()
                .ToList();

            return globalEnumTypes;
        }
    }
}