using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CSharp;

namespace FreePIE.Core.Plugins.MemoryMapping
{
    public interface IWorker
    {
        void Execute(IEnumerable<string> arguments);
    }

    public class WorkerProcess<TWorker> : IDisposable where TWorker : IWorker
    {
        private static CompilerResults WorkerExecutable;
        private static string[] WorkerSource = new[]
        {
                "using System;" +
                "using Test;" +
                "using System.Linq;" +
                "using System.Threading;" +
                "using System.Reflection;" +
                "class Program { static void Main(string[] args) { " +
                "Thread.Sleep(20000);" +
                "try {" +
                "var worker = Activator.CreateInstance(Type.GetType(args[0])) as IWorker;" +
                "worker.Execute(args.Skip(1));" +
                "} catch(Exception e) { throw e; }" +
                "} }"
        };

        private Process process;
        private string arguments;

        public WorkerProcess(string arguments = "")
        {
            this.arguments = arguments;
            Restart();
        }

        public bool IsAlive
        {
            get { return !process.HasExited; }
        }

        public void Restart()
        {
            if (process != null)
                process.Dispose();

            if (WorkerExecutable == null)
                WorkerExecutable = GenerateOrGetAssemblyForType();

            if (WorkerExecutable.Errors.HasErrors)
                throw new Exception("Error compiling worker executable: " + WorkerExecutable.Errors[0]);

            var processStartInfo = new ProcessStartInfo(WorkerExecutable.PathToAssembly)
            {
                Arguments = typeof(TWorker).AssemblyQualifiedName.Quote() + " " + arguments
            };

            process = Process.Start(processStartInfo);

            if (process.HasExited)
                throw new Exception("Worked process terminated prematurely: " + typeof(TWorker).FullName);
        }

        private static CompilerResults GenerateOrGetAssemblyForType()
        {
            var workerTypeAssemblyPath = Path.GetFullPath(typeof(TWorker).Assembly.Location);

            var codeProvider = new CSharpCodeProvider(new Dictionary<String, String> { { "CompilerVersion", "v4.0" } });


            var res = codeProvider.CompileAssemblyFromSource(new CompilerParameters
            {
                CompilerOptions = "/target:winexe",
                GenerateExecutable = true,
                OutputAssembly = Path.GetTempPath() + "FreePIE.WorkerHost." + typeof(TWorker).Name + "." + Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".exe",
                ReferencedAssemblies = { workerTypeAssemblyPath, typeof(EnumerableQuery).Assembly.Location, typeof(int).Assembly.Location }
            }, WorkerSource);

            File.Copy(workerTypeAssemblyPath, Path.Combine(Path.GetDirectoryName(res.PathToAssembly), Path.GetFileName(workerTypeAssemblyPath)), true);

            return res;
        }

        public void Dispose()
        {
            if (process != null)
            {
                process.Kill();
                process.Dispose();
            }
        }
    }

    public static class Extension
    {
        public static string Quote(this string input)
        {
            return "\"" + input + "\"";
        }
    }
}