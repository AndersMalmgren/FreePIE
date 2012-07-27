using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using FreePIE.Core.Services;
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

            SetupCustomMessageBindings();
        }

        protected override object GetInstance(Type service, string key)
        {
            return kernel.Get(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return kernel.GetAll(service);
        }

        private void SetupCustomMessageBindings()
        {
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
