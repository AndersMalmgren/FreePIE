using System;
using FreePIE.Core.Services;
using Ninject;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace FreePIE.Tests.Test
{
    public abstract class TestBase
    {
        private readonly StandardKernel kernel;

        public TestBase()
        {
            kernel = new StandardKernel();
            ServiceBootstrapper.AddCustomBindings(kernel);
        }

        protected T Get<T>()
        {
            return kernel.TryGet<T>();
        }

        protected IMethodOptions<object> WhenCalling<T>(Action<T> action) where T : class
        {
            return WhenCallingCheckForRegisterNew(action, false);
        }

        protected IMethodOptions<object> WhenCallingNewInstance<T>(Action<T> action) where T : class
        {
            return WhenCallingCheckForRegisterNew(action, true);
        }

        private IMethodOptions<object> WhenCallingCheckForRegisterNew<T>(Action<T> action, bool registerNew) where T : class
        {
            var instance = Get<T>();
            if (instance == null || registerNew)
                instance = Stub<T>();

            return instance.Stub(action);
        }

        protected T Stub<T>()  where T : class
        {
            var instance = MockRepository.GenerateMock<T>();
            kernel.Bind<T>().ToConstant(instance);
            return instance;
        }

        protected void Register<T>(T instance)
        {
            kernel.Bind<T>().ToConstant(instance);
        }


    }
}
