using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    public interface IPluginInvoker
    {
        IEnumerable<Type> ListAllPluginTypes();
        IEnumerable<IPlugin> InvokeAndConfigurePlugins(IEnumerable<Type> pluginTypes);
        void PopulatePluginSettings();
        IEnumerable<Type> ListAllGlobalEnumTypes();
    }
}