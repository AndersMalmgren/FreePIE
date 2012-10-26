using System.IO;
using System.Reflection;
using System.Text;

namespace FreePIE.Core.ScriptEngine.Python
{
    using System.Threading;
    using Common.Extensions;
    using Contracts;
    using Globals;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Scripting.Hosting;
    using IronPython.Hosting;

    public static class PythonExtensions
    {
        public static string AddReferences(this string script, params Assembly[] assemblies)
        {
            var builder = new StringBuilder();

            builder.AppendLine("import sys");
            builder.AppendLine("import clr");

            foreach (var assembly in assemblies)
            {
                builder.AppendLine(string.Format("sys.path.append(r'{0}')", Path.GetDirectoryName(assembly.Location)));
                builder.AppendLine(string.Format("clr.AddReferenceToFile(r'{0}')", Path.GetFileName(assembly.Location)));
            }

            return builder + Environment.NewLine + script;
        }

        public static string ImportNamespaces(this string script, params string[] namespaces)
        {
            var builder = new StringBuilder();

            foreach(var @namespace in namespaces)
                builder.Append(string.Format("from {0} import *{1}", @namespace, Environment.NewLine));

            return builder + Environment.NewLine + script;
        }
    }

    public static class DictionaryExtensions
    {
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            return pairs.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public class PythonScriptEngine : IScriptEngine
    {
        private readonly IScriptParser parser;
        private readonly IEnumerable<IGlobalProvider> globalProviders;
        private static ScriptEngine engine;
        private IEnumerable<IPlugin> usedPlugins;
        private volatile bool stopRequested;
        private const int LoopDelay = 1;
        private CountdownEvent pluginStopped;

        private static ScriptEngine Engine
        {
            get { return engine ?? (engine = Python.CreateEngine()); }
        }

        public PythonScriptEngine(IScriptParser parser, IEnumerable<IGlobalProvider> globalProviders)
        {
            this.parser = parser;
            this.globalProviders = globalProviders;
        }

        public void Start(string script)
        {
            thread = new Thread(obj1 => ExecuteSafe(() =>
            {
                var pluginStarted = new CountdownEvent(1);
                pluginStopped = new CountdownEvent(1);

                usedPlugins = parser.InvokeAndConfigureAllScriptDependantPlugins(script).ToList();

                var globals = CreateGlobals(usedPlugins, globalProviders);

                var scope = CreateScope(globals);

                foreach (var plugin in usedPlugins)
                    StartPlugin(plugin, pluginStarted, pluginStopped);

                pluginStopped.Signal();  
                pluginStarted.Signal();
                pluginStarted.Wait();

                script = PreProcessScript(script, usedPlugins, globals);

                ExecuteSafe(() =>
                {
                    while (!stopRequested)
                    {
                        usedPlugins.ForEach(p => p.DoBeforeNextExecute());
                        CatchThreadAbortedException(() => Engine.Execute(script, scope));
                        scope.SetVariable("starting", false);
                        Thread.Sleep(LoopDelay);
                    }
                });
            }));

            thread.Name = "PythonEngine Worker";
            thread.Start();
        }

        private void CatchThreadAbortedException(Action func)
        {
            try
            {
                func();
            } catch(ThreadAbortException e)
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

        private void StopPlugin(IPlugin obj)
        {
            ExecuteSafe(obj.Stop);
        }        

        string PreProcessScript(string script, IEnumerable<IPlugin> plugins, IDictionary<string, object> globals)
        {
            script = parser.PrepareScript(script, globals.Values);

            var pluginTypes = plugins.Select(x => x.GetType()).Select(x => new { x.Assembly, x.Namespace }).ToList();

            return script.ImportNamespaces(pluginTypes.Select(x => x.Namespace).ToArray())
                         .AddReferences(pluginTypes.Select(x => x.Assembly).ToArray());
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

            ThreadPool.QueueUserWorkItem(obj => OnError(this, new ScriptErrorEventArgs(e)));
        }

        private void OnError(object sender, ScriptErrorEventArgs e)
        {
            if (Error != null)
                Error(sender, e);
        }

        ScriptScope CreateScope(IDictionary<string, object> globals)
        {
            globals.Add("starting", true);
            return Engine.CreateScope(globals);
        }

        private CountdownEvent pluginStartedTemporary;
        private Thread thread;

        private void StartPlugin(IPlugin plugin, CountdownEvent pluginStarted, CountdownEvent pluginStopped)
        {
            var action = plugin.Start();

            if (action != null)
            {
                pluginStopped.AddCount();
                pluginStarted.AddCount();
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
            const int maximumShutdownTime = 200;

            DateTime stopRequestedTime = DateTime.Now;
            stopRequested = true;

            while((DateTime.Now - stopRequestedTime).TotalMilliseconds < maximumShutdownTime && thread.IsAlive)
            { }

            if(thread.IsAlive)
                thread.Abort();

            usedPlugins.ForEach(StopPlugin);
            pluginStopped.Wait();
        }

        public event EventHandler<ScriptErrorEventArgs> Error;
        
    }
}
