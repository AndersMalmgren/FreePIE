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
using FreePIE.Core.ScriptEngine.Globals;
using IronPython;
using IronPython.Compiler;
using IronPython.Hosting;
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

        public static string ImportNamespaces(this string script, params string[] namespaces)
        {
            var builder = new StringBuilder();

            foreach (var @namespace in namespaces)
                builder.Append(string.Format("from {0} import *{1}", @namespace, Environment.NewLine));

            if(namespaces.Length > 0)
                script = builder + Environment.NewLine + script;

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
        private static Microsoft.Scripting.Hosting.ScriptEngine engine;
        private IEnumerable<IPlugin> usedPlugins;
        private InterlockableBool stopRequested;
        private const int LoopDelay = 1;
        private CountdownEvent pluginStopped;

        private static Microsoft.Scripting.Hosting.ScriptEngine Engine
        {
            get { return engine ?? (engine = IronPython.Hosting.Python.CreateEngine()); }
        }

        private Parser CreatePythonParser(string script)
        {
            var src = HostingHelpers.GetSourceUnit(Engine.CreateScriptSourceFromString(script));
            return Parser.CreateParser(new CompilerContext(src, Engine.GetCompilerOptions(), ErrorSink.Default), new PythonOptions());
        }

        private static double StaticReferenceForCompilerOnly()
        {
            var x = IronPython.Modules.PythonMath.degrees(10);
            throw new InvalidOperationException("Do not use this method - it is only to force local copy.");
        }

        public PythonScriptEngine(IScriptParser parser, IEnumerable<IGlobalProvider> globalProviders, IEventAggregator eventAggregator)
        {
            this.parser = parser;
            this.globalProviders = globalProviders;
            this.eventAggregator = eventAggregator;
        }

        public void Start(string script)
        {
            thread = new Thread(obj1 => ExecuteSafe(() =>
            {
                var pluginStarted = new CountdownEvent(0);

                pluginStopped = new CountdownEvent(0);

                usedPlugins = parser.InvokeAndConfigureAllScriptDependantPlugins(script).ToList();

                var globals = CreateGlobals(usedPlugins, globalProviders);

                var pluginTypes = usedPlugins.Select(x => x.GetType()).Select(x => new { x.Assembly, x.Namespace }).ToList();

                Engine.Runtime.AddReferences(pluginTypes.Select(x => x.Assembly).Distinct().ToArray());

                var scope = CreateScope(globals);
                
                foreach (var plugin in usedPlugins)
                    StartPlugin(plugin, pluginStarted, pluginStopped);

                pluginStarted.Wait();

                Engine.SetSearchPaths(GetPythonPaths());

                script = PreProcessScript(script, usedPlugins, globals);

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
                    Thread.Sleep(16);
                }
                scope.SetVariable("stopping", true);
                CatchThreadAbortedException(() => compiled.Execute(scope));
            });
        }

        ICollection<string> GetPythonPaths()
        {
            return new Collection<string> { Path.Combine(Environment.CurrentDirectory, "pylib") };
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

        string PreProcessScript(string script, IEnumerable<IPlugin> plugins, IDictionary<string, object> globals)
        {
            script = parser.PrepareScript(script, globals.Values);
            var namespaces = plugins.Select(x => x.GetType()).Select(x => x.Namespace).ToArray();
            return script.ImportNamespaces(namespaces);
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
                .FirstOrDefault();

            ThreadPool.QueueUserWorkItem(obj =>
                {
                    try
                    {
                        Stop();
                    }
                    finally
                    {
                        eventAggregator.Publish(new ScriptErrorEvent(e, lineNumber));
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
            
            usedPlugins.ForEach(p => StopPlugin(p, pluginStopped));
            pluginStopped.Wait();
        }
    }
}
