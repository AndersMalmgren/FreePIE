using System;
using System.Linq;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Common.StructureMap;
using FreePIE.Core.Persistence;
using FreePIE.Core.Persistence.Paths;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Core.ScriptEngine.Python;
using FreePIE.Core.ScriptEngine.ThreadTiming;
using StructureMap;

namespace FreePIE.Core.Services
{
    public static class ServiceBootstrapper
    {
        public static IContainer Create()
        {
	        var container = new Container(config =>
	        {
				ConfigScriptEngine(config);
				ConfigPersistance(config);
				ConfigGlobalsProviders(config);
				ConfigCommon(config);
	        });

			return container;
        }

		private static void ConfigCommon(ConfigurationExpression config)
		{
			config.For(typeof (IFactory<>)).Use(typeof (StructureMapFactory<>));
			config.For<IEventAggregator>().Singleton().Use<EventAggregator>();
            config.For<IFileSystem>().Use<FileSystem>();
            config.For<ILog>().Use<Log>();
        }

		private static void ConfigGlobalsProviders(ConfigurationExpression config)
        {
            config.For<IGlobalProvider>().Use<ScriptHelpersGlobalProvider>();
            config.For<IGlobalProvider>().Use<CurveGlobalProvider>();
        }

		private static void ConfigPersistance(ConfigurationExpression config)
        {
			config.For<ISettingsManager>().Singleton().Use<SettingsManager>();
			config.For<IPersistanceManager>().Singleton().Use<PersistanceManager>();
			config.For<IPluginInvoker>().Singleton().Use<PluginInvoker>();

            config.For<IPortable>().Use<Portable>();
			config.For<IPaths>().Singleton().Use("ProviderFactory", GetPath);
        }

		private static void ConfigScriptEngine(ConfigurationExpression config)
        {
            config.For<IScriptEngine>().Use<PythonScriptEngine>();
            config.For<IScriptParser>().Use<PythonScriptParser>();
            config.For<ICodeCompletionProvider>().Use<CodeCompletionProvider>();
            config.For<IRuntimeInfoProvider>().Use<RuntimeInfoProvider>();
			config.For<IThreadTimingFactory>().Singleton().Use<ThreadTimingFactory>();
        }

	    private static IPaths GetPath(IContext context)
	    {
		    var portable = context.GetInstance<IPortable>();

			return portable.IsPortable ? context.GetInstance<PortablePaths>() : context.GetInstance<UacCompliantPaths>() as IPaths;
	    }
    }
}
