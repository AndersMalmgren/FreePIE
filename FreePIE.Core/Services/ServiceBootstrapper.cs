using System;
using System.Linq;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Persistence;
using FreePIE.Core.Persistence.Paths;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Core.ScriptEngine.Python;
using FreePIE.Core.ScriptEngine.ThreadTiming;
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

            BindScriptEngine(kernel);
            BindPersistance(kernel);
            BindGlobalsProviders(kernel);
            BindCommon(kernel);

            return kernel;
        }

        private static void BindCommon(StandardKernel kernel)
        {
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            kernel.Bind<IFileSystem>().To<FileSystem>();
            kernel.Bind<ILog>().To<Log>();
        }

        private static void BindGlobalsProviders(StandardKernel kernel)
        {
            kernel.Bind<IGlobalProvider>().To<ScriptHelpersGlobalProvider>();
            kernel.Bind<IGlobalProvider>().To<CurveGlobalProvider>();
        }

        private static void BindPersistance(StandardKernel kernel)
        {
            kernel.Bind<ISettingsManager>().To<SettingsManager>().InSingletonScope();
            kernel.Bind<IPersistanceManager>().To<PersistanceManager>();
            kernel.Bind<IPluginInvoker>().To<PluginInvoker>().InSingletonScope();

            kernel.Bind<IPortable>().To<Portable>();
            kernel.Bind<IPaths>().ToProvider<PathsProvider>().InSingletonScope();
        }

        private static void BindScriptEngine(StandardKernel kernel)
        {
            kernel.Bind<IScriptEngine>().To<PythonScriptEngine>();
            kernel.Bind<IScriptParser>().To<PythonScriptParser>();
            kernel.Bind<ICodeCompletionProvider>().To<CodeCompletionProvider>();
            kernel.Bind<IRuntimeInfoProvider>().To<RuntimeInfoProvider>();
            kernel.Bind<IThreadTimingFactory>().To<ThreadTimingFactory>().InSingletonScope();
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

        private class GenericFunctionFactory<T>
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

        private class FunctionFactory<T, TCast> where T : Type where TCast : class
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

        private sealed class PathsProvider : IProvider<IPaths>
        {
            private readonly IPortable portable;
            private readonly Func<PortablePaths> portablePaths;
            private readonly Func<UacCompliantPaths> uacPaths;

            public PathsProvider(IPortable portable, Func<PortablePaths> portablePaths, Func<UacCompliantPaths>  uacPaths)
            {
                this.portable = portable;
                this.portablePaths = portablePaths;
                this.uacPaths = uacPaths;
            }

            public object Create(IContext context)
            {
                return portable.IsPortable ? portablePaths() : uacPaths() as IPaths;
            }

            public Type Type { get; private set; }
        }
    }
}
