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

    public class PythonScriptEngine : IScriptEngine
    {
        private readonly IScriptParser parser;
        private readonly IEnumerable<IGlobalProvider> globalProviders;
        private readonly ScriptEngine engine;
        private IEnumerable<IPlugin> usedPlugins;
        private bool stopRequested;
        private const int LoopDelay = 1;

        public PythonScriptEngine(IScriptParser parser, IEnumerable<IGlobalProvider> globalProviders)
        {
            this.parser = parser;
            this.globalProviders = globalProviders;
            engine = Python.CreateEngine();
        }

        public void Start(string script)
        {
            ThreadPool.QueueUserWorkItem(obj1 => ExecuteSafe(() =>
            {
                var pluginStarted = new CountdownEvent(1);
                var pluginStopped = new CountdownEvent(1);

                usedPlugins = parser.InvokeAndConfigureAllScriptDependantPlugins(script).ToList();

                var scope = InitSession(usedPlugins);

                foreach (var plugin in usedPlugins)
                    StartPlugin(plugin, pluginStarted, pluginStopped);

                pluginStopped.Signal();  
                pluginStarted.Signal();
                pluginStarted.Wait();

                script = PreProcessScript(script, usedPlugins);

                while (!stopRequested)
                {
                    usedPlugins.ForEach(p => p.DoBeforeNextExecute());
                    engine.Execute(script, scope);
                    scope.SetVariable("starting", false);
                    Thread.Sleep(LoopDelay);
                }

                usedPlugins.ForEach(StopPlugin);
                pluginStopped.Wait();
            }));
        }

        private void StopPlugin(IPlugin obj)
        {
            ExecuteSafe(obj.Stop);
        }        

        string PreProcessScript(string script, IEnumerable<IPlugin> plugins)
        {
            var pluginTypes = plugins.Select(x => x.GetType()).Select(x => new { Assembly = x.Assembly, Namespace = x.Namespace }).ToList();

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
                TriggerErrorEventNotOnLuaThread(e);
            }
        }

        private void TriggerErrorEventNotOnLuaThread(Exception e)
        {
            if (e.InnerException != null)
            {
                TriggerErrorEventNotOnLuaThread(e.InnerException);
                return;
            }

            OnError(this, new ScriptErrorEventArgs(e));
        }

        private void OnError(object sender, ScriptErrorEventArgs e)
        {
            if (Error != null)
                Error(sender, e);
        }

        ScriptScope InitSession(IEnumerable<IPlugin> plugins)
        {
            var globals = globalProviders.SelectMany(gp => gp.ListGlobals()).ToList();

            var types = plugins.ToDictionary(GlobalsInfo.GetGlobalName, x => x.CreateGlobal())
                                    .Union(globals.ToDictionary(GlobalsInfo.GetGlobalName, x => x))
                                    .Union(new [] { new KeyValuePair<string, object>("starting", true) })
                                    .ToDictionary(x => x.Key, x => x.Value);

            return engine.CreateScope(types);
        }

        private CountdownEvent pluginStartedTemporary;

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
            stopRequested = true;
        }

        public event EventHandler<ScriptErrorEventArgs> Error;
    }
}
