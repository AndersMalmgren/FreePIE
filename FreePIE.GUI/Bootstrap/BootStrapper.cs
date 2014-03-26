using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using FreePIE.Core.Common;
using FreePIE.Core.Persistence;
using FreePIE.Core.Services;
using FreePIE.GUI.Common.AvalonDock;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells;
using Ninject;

namespace FreePIE.GUI.Bootstrap
{
    public class BootStrapper : Bootstrapper<MainShellViewModel>
    {
        private IKernel kernel;

        protected override void Configure()
        {
            kernel = ServiceBootstrapper.Create();
            kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
            kernel.Bind<IResultFactory>().To<ResultFactory>();
            kernel.Bind<IPaths>().To<UacCompliantPaths>().InSingletonScope();

            SetupCustomMessageBindings();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            kernel.Get<IPersistanceManager>().Load();
            base.OnStartup(sender, e);
        }

        protected override object GetInstance(Type service, string key)
        {
            return kernel.Get(service);
        }
        
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return kernel.GetAll(service);
        }

        protected override void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log(e.Exception, 0);
            base.OnUnhandledException(sender, e);
        }

        private string PrependTabsToLinebreaks(string input, int numberOfTabs)
        {
            var tabs = new string('\t', numberOfTabs);

            return tabs + input.Replace(Environment.NewLine, Environment.NewLine + tabs);
        }

        private void Log(Exception e, int indentation)
        {
            if (e == null)
                return;

            var fileSystem = kernel.Get<IFileSystem>();
            var paths = kernel.Get<IPaths>();

            var path = paths.GetDataPath("FreePIE.log");

            var delimiter = indentation == 0 ? DateTime.Now.ToString() + " - " : string.Empty;

            var log = string.Format("{0}{1}{2} - {3}: {5}{4}{5}", new string('\t', indentation), delimiter, e.GetType().FullName, e.Message, PrependTabsToLinebreaks(e.StackTrace, indentation), Environment.NewLine);
            fileSystem.AppendAllText(path, log);

            Log(e.InnerException, indentation + 1);

            if (indentation == 0)
                fileSystem.AppendAllText(path, Environment.NewLine);
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
