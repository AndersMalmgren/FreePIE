using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Common.Extensions;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model.Events;
using FreePIE.Core.Persistence;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Core.ScriptEngine.ThreadTiming;
using IronPython;
using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Modules;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;

namespace FreePIE.Core.ScriptEngine.Python
{
    public static class PythonExtensions
    {
        public static void AddReferences(this ScriptRuntime runtime, params Assembly[] assemblies)
        {
            foreach(var assembly in assemblies)
                runtime.LoadAssembly(assembly);
        }

        public static string ImportTypes(this string script, IEnumerable<Type> types, out int startingLine)
        {
            startingLine = 0;
            var builder = new StringBuilder();

            foreach (var type in types)
            {
                startingLine++;
                builder.Append(string.Format("from {0} import {1}{2}", type.Namespace, type.Name, Environment.NewLine));
            }

            if (types.Any())
            {
                startingLine++;
                script = builder + Environment.NewLine + script;
            }

            return script;
        }
    }

    public static class Extensions
    {
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            return pairs.ToDictionary(x => x.Key, x => x.Value);
        }

        public static void Increment(this CountdownEvent @event)
        {
            if(@event.IsSet)
                @event.Reset(1);
            else @event.AddCount();
        }
    }

    public class PythonScriptEngine : IScriptEngine
    {
        private readonly IScriptParser parser;
        private readonly IEnumerable<IGlobalProvider> globalProviders;
        private readonly IEventAggregator eventAggregator;
        private readonly IThreadTimingFactory threadTimingFactory;
        private readonly IPaths paths;
        private static Microsoft.Scripting.Hosting.ScriptEngine engine;
        private IEnumerable<IPlugin> usedPlugins;
        private InterlockableBool stopRequested;
        private const int LoopDelay = 1;
        private CountdownEvent pluginStopped;
        private int startingLine;

        static PythonScriptEngine()
        {
            var x = typeof (PythonMath);
            if(x == null)
                throw new Exception("Do not remove - Used to force local copy.");
        }

        private static Microsoft.Scripting.Hosting.ScriptEngine Engine
        {
            get { return engine ?? (engine = IronPython.Hosting.Python.CreateEngine()); }
        }

        public PythonScriptEngine(IScriptParser parser, IEnumerable<IGlobalProvider> globalProviders, IEventAggregator eventAggregator, IThreadTimingFactory threadTimingFactory, IPaths paths)
        {
            this.parser = parser;
            this.globalProviders = globalProviders;
            this.eventAggregator = eventAggregator;
            this.threadTimingFactory = threadTimingFactory;
            this.paths = paths;
        }

        public void Start(string script)
        {
            thread = new Thread(obj1 => ExecuteSafe(() =>
            {
                threadTimingFactory.SetDefault();

                var pluginStarted = new CountdownEvent(0);

                pluginStopped = new CountdownEvent(0);

                usedPlugins = parser.InvokeAndConfigureAllScriptDependantPlugins(script).ToList();

                var usedGlobalEnums = parser.GetAllUsedGlobalEnums(script);

                var globals = CreateGlobals(usedPlugins, globalProviders);

                Engine.Runtime.AddReferences(usedPlugins.Select(x => x.GetType().Assembly).Concat(usedGlobalEnums.Select(t => t.Assembly)).Distinct().ToArray());

                var scope = CreateScope(globals);

                foreach (var plugin in usedPlugins)
                    StartPlugin(plugin, pluginStarted, pluginStopped);

                pluginStarted.Wait();

                Engine.SetSearchPaths(GetPythonPaths());
                
                script = PreProcessScript(script, usedGlobalEnums, globals);

                RunLoop(Engine.CreateScriptSourceFromString(script).Compile(), scope);
            })) {Name = "PythonEngine Worker"};

            thread.Start();
        }

        void RunLoop(CompiledCode compiled, ScriptScope scope)
        {
            ExecuteSafe(() =>
            {
                while (!stopRequested)
                {
                    usedPlugins.ForEach(p => p.DoBeforeNextExecute());
                    CatchThreadAbortedException(() => compiled.Execute(scope));
                    scope.SetVariable("starting", false);
                    threadTimingFactory.Get().Wait();
                }
                scope.SetVariable("stopping", true);
                CatchThreadAbortedException(() => compiled.Execute(scope));
            });
        }

        ICollection<string> GetPythonPaths()
        {
            return new Collection<string> { paths.GetApplicationPath("pylib") };
        }

        private void CatchThreadAbortedException(Action func)
        {
            try
            {
                func();
            } catch(ThreadAbortException)
            {
                Thread.ResetAbort();
                throw new Exception("Had to forcibly shut down script - try removing infinite loops from the script");
            }
        }

        private static IDictionary<string, object> CreateGlobals(IEnumerable<IPlugin> plugins, IEnumerable<IGlobalProvider> providers)
        {
            var globals = providers.SelectMany(gp => gp.ListGlobals()).ToList();

            var types = plugins.ToDictionary(GlobalsInfo.GetGlobalName, x => x.CreateGlobal())
                               .Union(globals.ToDictionary(GlobalsInfo.GetGlobalName, x => x))
                               .ToDictionary();

            return types;
        }

        private void StopPlugin(IPlugin obj, CountdownEvent @event)
        {
            ExecuteSafe(obj.Stop);
            if(@event.CurrentCount > 0)
                @event.Signal();
        }

        private string PreProcessScript(string script, IEnumerable<Type> globalEnums, IDictionary<string, object> globals)
        {
            parser.ListDeprecatedWarnings(script, globals.Values).ForEach(eventAggregator.Publish);

            script = parser.PrepareScript(script, globals.Values);
            return script.ImportTypes(globalEnums, out startingLine);
        }

        void ExecuteSafe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                TriggerErrorEventNotOnPythonThread(e);
            }
        }

        private void TriggerErrorEventNotOnPythonThread(Exception e)
        {
            if (e.InnerException != null)
            {
                TriggerErrorEventNotOnPythonThread(e.InnerException);
                return;
            }

            var stack = PythonOps.GetDynamicStackFrames(e);
            var lineNumber = stack
                .Select(s => (int?)s.GetFileLineNumber())
                .FirstOrDefault() - startingLine;

            ThreadPool.QueueUserWorkItem(obj =>
                {
                    try
                    {
                        Stop();
                    }
                    finally
                    {
                        eventAggregator.Publish(new ScriptErrorEvent(ErrorLevel.Exception, e.Message, lineNumber));
                    }
                });
        }

        private ScriptScope CreateScope(IDictionary<string, object> globals)
        {
            globals.Add("starting", true);
            globals.Add("stopping", false);

            var scope = Engine.CreateScope(globals);
            scope.ImportModule("math");
            return scope;
        }

        private CountdownEvent pluginStartedTemporary;
        private Thread thread;

        private void StartPlugin(IPlugin plugin, CountdownEvent pluginStarted, CountdownEvent pluginStopped)
        {
            var action = plugin.Start();

            pluginStopped.Increment();

            if (action != null)
            {
                
                pluginStarted.Increment();
                pluginStartedTemporary = pluginStarted;
                plugin.Started += WhenPluginHasStarted;
                ThreadPool.QueueUserWorkItem(x => ExecuteSafe(action));
            }
        }

        private void WhenPluginHasStarted(object sender, EventArgs args)
        {
            (sender as IPlugin).Started -= WhenPluginHasStarted;
            pluginStartedTemporary.Signal();
        }

        public void Stop()
        {
            var wasStopRequested = stopRequested.CompareExchange(true, false);

            if (wasStopRequested)
                return;

            const int maximumShutdownTime = 200;

            DateTime stopRequestedTime = DateTime.Now;

            while((DateTime.Now - stopRequestedTime).TotalMilliseconds < maximumShutdownTime && thread.IsAlive)
            { }

            if(thread.IsAlive)
                thread.Abort();

            threadTimingFactory.Get().Dispose();
            usedPlugins.ForEach(p => StopPlugin(p, pluginStopped));
            pluginStopped.Wait();
        }
    }
}
