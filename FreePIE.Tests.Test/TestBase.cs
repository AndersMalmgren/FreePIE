using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        protected void AssertDouble(double expected, double actual)
        {
            int magnitude = 1 + (Math.Abs(expected) < 1E-15 ? -1 : Convert.ToInt32(Math.Floor(Math.Log10(Math.Abs(expected)))));
            int precision = 15 - magnitude;

            double tolerance = 1.0 / Math.Pow(10, precision);
            Assert.IsTrue(Math.Abs(expected - actual) <= tolerance, "{0} <> {1}", expected, actual);
        }

    }
}
