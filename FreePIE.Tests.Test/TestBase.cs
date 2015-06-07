using System;
using FreePIE.Core.Services;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using StructureMap;

namespace FreePIE.Tests.Test
{
    public abstract class TestBase
    {
        private readonly IContainer container;

        protected TestBase()
        {
			container = new Container();
        }

        protected T Get<T>()
        {
			return container.TryGetInstance<T>();
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
	        Register(instance);
            return instance;
        }

		protected void Register<T>(T instance) where T : class
        {
            container.Configure(config =>  config.For<T>().Use(instance));
        }


    }
}
