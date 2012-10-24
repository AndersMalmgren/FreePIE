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
        public static void AddReference(this ScriptEngine engine, params Assembly[] assemblies)
        {
            var builder = new StringBuilder();
            builder.AppendLine("import sys");
            builder.AppendLine("import clr");
            foreach (var assembly in assemblies)
            {
                builder.AppendLine(string.Format("sys.path.append(r'{0}')", Path.GetDirectoryName(assembly.Location)));
                builder.AppendLine(string.Format("clr.AddReferenceToFile(r'{0}')", Path.GetFileName(assembly.Location)));
            }

            engine.Execute(builder.ToString());
        }

        public static void ImportNamespace(this ScriptEngine engine, string @namespace)
        {
            engine.Execute(string.Format("from {0} import *", @namespace));
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
                var pluginsStopped = new CountdownEvent(0);

                usedPlugins = parser.InvokeAndConfigureAllScriptDependantPlugins(script).ToList();

                var scope = InitSession(usedPlugins);

                foreach (var plugin in usedPlugins)
                    StartPlugin(plugin, pluginsStopped);

                script = PreProcessScript(script);

                while (!stopRequested)
                {
                    usedPlugins.ForEach(p => p.DoBeforeNextExecute());
                    engine.Execute(script, scope);
                    scope.SetVariable("starting", false);
                    Thread.Sleep(LoopDelay);
                }

                ThreadPool.QueueUserWorkItem(obj2 => usedPlugins.ForEach(p => p.Stop()));
                pluginsStopped.Wait();
            }));
        }

        string PreProcessScript(string script)
        {
            return script;
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

        private void StartPlugin(IPlugin plugin, CountdownEvent pluginsStopped)
        {
            var action = plugin.Start();

            if (action != null)
            {
                ThreadPool.QueueUserWorkItem(x => ExecuteSafe(action));
                pluginsStopped.AddCount();
            }
        }

        public void Stop()
        {
            stopRequested = true;
        }

        public event EventHandler<ScriptErrorEventArgs> Error;
    }
}
