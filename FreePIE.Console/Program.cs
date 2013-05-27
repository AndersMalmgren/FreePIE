using FreePIE.Core.Persistence;
using FreePIE.Core.Services;
using Ninject;

namespace FreePIE.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var kernel = ServiceBootstrapper.Create();
            kernel.Bind<IPaths>().To<UacCompliantPaths>().InSingletonScope();
            kernel.Get<ConsoleHost>().Start(args);
        }
    }
}
