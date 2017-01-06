using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using FreePIE.Core.Persistence;
using FreePIE.Core.Services;
using FreePIE.GUI.Common.AvalonDock;
using FreePIE.GUI.Common.CommandLine;
using FreePIE.GUI.Common.TrayIcon;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells;
using FreePIE.GUI.Views.Main;
using FreePIE.GUI.Views.Script.Output;
using Ninject;
using ILog = FreePIE.Core.Common.ILog;
using Parser = FreePIE.GUI.Common.CommandLine.Parser;

namespace FreePIE.GUI.Bootstrap
{
    public class Bootstrapper : BootstrapperBase
    {
        private IKernel kernel;

        public Bootstrapper()
        {
            Initialize();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        protected override void Configure()
        {
            kernel = ServiceBootstrapper.Create();
            kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
            kernel.Bind<IResultFactory>().To<ResultFactory>();
            kernel.Bind<IParser>().To<Parser>();
            kernel.Bind<ITrayIcon>().To<TrayIconViewModel>().InSingletonScope();
            ConfigurePanels();

            SetupCustomMessageBindings();
        }

	    private void ConfigurePanels()
	    {
		    kernel.Bind<PanelViewModel>().To<ConsoleViewModel>();
			kernel.Bind<PanelViewModel>().To<ErrorsViewModel>();
			kernel.Bind<PanelViewModel>().To<WatchesViewModel>();
	    }

	    protected override void OnStartup(object sender, StartupEventArgs e)
	    {
	        Coroutine.BeginExecute(kernel
                .Get<SettingsLoaderViewModel>()
                //.Load(() => DisplayRootViewFor<MainShellViewModel>())
                .Load(() => DisplayRootViewFor<ITrayIcon>())
                .GetEnumerator());
        }

        protected override object GetInstance(Type service, string key)
        {
            return kernel.Get(service);
        }
        
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return kernel.GetAll(service);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            kernel.Get<ILog>().Error(e.ExceptionObject as Exception);
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
