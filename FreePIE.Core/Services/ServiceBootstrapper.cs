using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Persistence;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.Core.ScriptEngine.Globals;
using Ninject;
using Ninject.Activation;
using Ninject.Parameters;

namespace FreePIE.Core.Services
{
    public static class ServiceBootstrapper
    {
        public static IKernel Create()
        {
            var kernel = new StandardKernel();
            AddCustomBindings(kernel);

            kernel.Bind<IScriptEngine>().To<LuaEngine>();
            kernel.Bind<IScriptParser>().To<LuaScriptParser>();
            kernel.Bind<ICodeCompletionProvider>().To<CodeCompletionProvider>();
            kernel.Bind<IRuntimeInfoProvider>().To<RuntimeInfoProvider>();

            kernel.Bind<ISettingsManager>().To<SettingsManager>().InSingletonScope();
            kernel.Bind<IPersistanceManager>().To<PersistanceManager>();
            kernel.Bind<IPluginInvoker>().To<PluginInvoker>().InSingletonScope();


            kernel.Bind<IGlobalProvider>().To<ScriptHelpersGlobalProvider>();
            kernel.Bind<IGlobalProvider>().To<CurveGlobalProvider>();

            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            kernel.Bind<IFileSystem>().To<FileSystem>();


            return kernel;
        }

        public static void AddCustomBindings(IKernel kernel)
        {
            //Abstract ninject for invokers
            kernel.Bind(typeof (Func<>)).ToMethod(CreateGenericFunc).When(VerifyGenericFactoryFunction);
            kernel.Bind(typeof(Func<,>)).ToMethod(CreateFunc).When(VerifyFactoryFunction);
        }

        private static bool VerifyFactoryFunction(IRequest request)
        {
            var genericArguments = request.Service.GetGenericArguments();
            if (genericArguments.Count() > 2)
            {
                return false;
            }

            return true;

        }

        private static object CreateFunc(IContext ctx)
        {
            var functionFactoryType = typeof(FunctionFactory<,>).MakeGenericType(ctx.GenericArguments);
            var ctor = functionFactoryType.GetConstructors().Single();
            var functionFactory = ctor.Invoke(new object[] { ctx.Kernel });
            return functionFactoryType.GetMethod("Create").Invoke(functionFactory, new object[0]);
        }

        private static bool VerifyGenericFactoryFunction(IRequest request)
        {
            var genericArguments = request.Service.GetGenericArguments();
            if (genericArguments.Count() != 1)
            {
                return false;
            }

            var instanceType = genericArguments.Single();
            return request.ParentContext.Kernel.CanResolve(new Request(genericArguments[0], null, new IParameter[0], null, false, true)) ||
                   TypeIsSelfBindable(instanceType);
        }

        private static object CreateGenericFunc(IContext ctx)
        {
            var functionFactoryType = typeof(GenericFunctionFactory<>).MakeGenericType(ctx.GenericArguments);
            var ctor = functionFactoryType.GetConstructors().Single();
            var functionFactory = ctor.Invoke(new object[] { ctx.Kernel });
            return functionFactoryType.GetMethod("Create").Invoke(functionFactory, new object[0]);
        }

        private static bool TypeIsSelfBindable(Type service)
        {
            return !service.IsInterface
                   && !service.IsAbstract
                   && !service.IsValueType
                   && service != typeof(string)
                   && !service.ContainsGenericParameters;
        }

        public class GenericFunctionFactory<T>
        {
            private readonly IKernel kernel;

            public GenericFunctionFactory(IKernel kernel)
            {
                this.kernel = kernel;
            }

            public Func<T> Create()
            {
                return () => this.kernel.Get<T>();
            }
        }

        public class FunctionFactory<T, TCast> where T : Type where TCast : class
        {
            private readonly IKernel kernel;

            public FunctionFactory(IKernel kernel)
            {
                this.kernel = kernel;
            }

            public Func<T, TCast> Create()
            {
                return t => this.kernel.Get(t) as TCast;
            }
        }
    }
}
