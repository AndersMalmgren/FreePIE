using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
        }

        protected override object GetInstance(Type service, string key)
        {
            return kernel.Get(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return kernel.GetAll(service);
        }
    }
}
