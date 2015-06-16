using FreePIE.Core.Persistence;
using FreePIE.Core.Persistence.Paths;
using FreePIE.Core.Services;

namespace FreePIE.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var container = ServiceBootstrapper.Create();
            container.GetInstance<ConsoleHost>().Start(args);
        }
    }
}
