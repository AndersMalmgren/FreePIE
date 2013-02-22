using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            kernel.Get<IPersistanceManager>().Load();
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
            Log(e.Exception);
            base.OnUnhandledException(sender, e);
        }

        private void Log(Exception e)
        {
            if (e == null) return;

            var fileSystem = kernel.Get<IFileSystem>();
            var log = string.Format("{0} - {1}: {2}{3}{3}", DateTime.Now, e.Message, e.StackTrace, Environment.NewLine);
            fileSystem.AppendAllText("FreePIE.log", log);

            Log(e.InnerException);
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
