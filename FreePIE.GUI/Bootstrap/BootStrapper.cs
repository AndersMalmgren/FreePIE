using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using FreePIE.Core.Persistence;
using FreePIE.Core.Services;
using FreePIE.GUI.Common.AvalonDock;
using FreePIE.GUI.Common.CommandLine;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells;
using FreePIE.GUI.Views.Main;
using FreePIE.GUI.Views.Script.Output;
using StructureMap;
using ILog = FreePIE.Core.Common.ILog;
using Parser = FreePIE.GUI.Common.CommandLine.Parser;

namespace FreePIE.GUI.Bootstrap
{
    public class Bootstrapper : BootstrapperBase
    {
        private IContainer container;

        public Bootstrapper()
        {
            Initialize();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        protected override void Configure()
        {
			container = ServiceBootstrapper.Create();
			container.Configure(config =>
			{
				config.For<IWindowManager>().Singleton().Use<WindowManager>();
				config.For<IResultFactory>().Use<ResultFactory>();
				config.For<IParser>().Use<Parser>();

				ConfigurePanels(config);
			});



            SetupCustomMessageBindings();
        }

	    private void ConfigurePanels(ConfigurationExpression config)
	    {
			config.For<PanelViewModel>().Use<ConsoleViewModel>();
			config.For<PanelViewModel>().Use<ErrorsViewModel>();
			config.For<PanelViewModel>().Use<WatchesViewModel>();
	    }

	    protected override void OnStartup(object sender, StartupEventArgs e)
        {
            container.GetInstance<IPersistanceManager>().Load();
            DisplayRootViewFor<MainShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
			return container.GetInstance(service);
        }
        
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service).Cast<object>();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            container.GetInstance<ILog>().Error(e.ExceptionObject as Exception);
        }

        private void SetupCustomMessageBindings()
        {
            DocumentContext.Init();
            MessageBinder.SpecialValues.Add("$orignalsourcecontext", context =>
            {
                var args = context.EventArgs as RoutedEventArgs;
                if (args == null)
                {
                    return null;
                }

                var fe = args.OriginalSource as FrameworkElement;
                if (fe == null)
                {
                    return null;
                }

                return fe.DataContext;
            });            
        }
    }
}
