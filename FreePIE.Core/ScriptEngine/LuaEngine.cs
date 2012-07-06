using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using FreePIE.Core.Common.Extensions;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine.Globals;
using LuaInterface;

namespace FreePIE.Core.ScriptEngine
{
    public class LuaEngine : IScriptEngine
    {
        private readonly IEnumerable<IGlobalProvider> globalProviders;
        private readonly IScriptParser scriptParser;
        private readonly IPluginInvoker pluginInvoker;
        private readonly Lua lua;
        private readonly BackgroundWorker luaWorker;
        private bool running = false;
        private bool error = false;
        private bool pluginsAreStarting = false;
        private string script;
        private IEnumerable<IPlugin> usedPlugins;
        

        public LuaEngine(IEnumerable<IGlobalProvider> globalProviders, IScriptParser scriptParser, IPluginInvoker pluginInvoker)
        {
            this.globalProviders = globalProviders;
            this.scriptParser = scriptParser;
            this.pluginInvoker = pluginInvoker;
            luaWorker = new BackgroundWorker();
            lua = new Lua();  
        }

        public void Start(string script)
        {
            this.script = script;

            InitPlugins();
            var globals = InitGlobals();
            PrepareScriptForGlobals(globals);
            RegisterEnumerations();

            stopSync = new AutoResetEvent(false);
            luaWorker.DoWork += LuaWorker;
            luaWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            WaitForSlowStartingPlugins();
            StopLuaEngineAndWaitUntilStopped();
            StopPlugins();
            WaitForThreadedPluginsToStop();
            lua.Dispose();
        }

        public event EventHandler<ScriptErrorEventArgs> Error;

        protected virtual void OnError(object sender, ScriptErrorEventArgs e)
        {
            if (Error != null)
                Error(sender, e);
        }

        private void TriggerErrorEventNotOnLuaThread(Exception e)
        {
            if(e.InnerException != null)
            {
                TriggerErrorEventNotOnLuaThread(e.InnerException);
                return;
            }
            ThreadPool.QueueUserWorkItem(obj => OnError(this, new ScriptErrorEventArgs(e)));
        }

        private void ExecuteSafe(Action protectedAction)
        {
            try
            {
                protectedAction();
            }
            catch (Exception e)
            {
                error = true;
                TriggerErrorEventNotOnLuaThread(e);
            }
        }

        private void LuaWorker(object sender, DoWorkEventArgs e)
        {
            running = true;
            bool starting = true;
            lua["starting"] = starting;
            while(running)
            {
                if (!error)
                {
                    ExecuteSafe(() =>
                    {
                        usedPlugins.ForEach(p => p.DoBeforeNextExecute());
                        lua.DoString(script);
                    });
                        
                    if (starting)
                    {
                        starting = false;
                        lua["starting"] = starting;
                    }
                }

                Thread.Sleep(1);
            }

            stopSync.Set();
        }

        private int threadedPlugins;
        private int threadedPluginStarting;
        private int threadedPluginStopping;

        private AutoResetEvent threadSync;
        private AutoResetEvent stopSync;

        private void InitPlugins()
        {
            pluginsAreStarting = true;
            usedPlugins = scriptParser.InvokeAndConfigureAllScriptDependantPlugins(script);

            threadedPluginStarting = 0;
            threadedPlugins = 0;
            threadSync = new AutoResetEvent(false);

            foreach (var plugin in usedPlugins)
            {
                ExecuteSafe(() =>
                    {
                        var name = GlobalsInfo.GetGlobalName(plugin);
                        lua[name] = plugin.CreateGlobal();

                        StartPlugin(plugin);
                    });
            }

            if (threadedPluginStarting > 0)
                threadSync.WaitOne();

            pluginsAreStarting = false;
        }

        private void StartPlugin(IPlugin plugin)
        {
            var threadedAction = plugin.Start();
            if (threadedAction != null)
            {
                threadedPlugins++;
                threadedPluginStarting++;
                plugin.Started += PluginStarted;
                ThreadPool.QueueUserWorkItem((obj) => ThreadedPluginHandler(threadedAction));
            }
        }

        private void ThreadedPluginHandler(Action pluginThreadedAction)
        {
            try
            {
                pluginThreadedAction();
            }
            catch (Exception e)
            {
                TriggerErrorEventNotOnLuaThread(e);
                error = true;
                threadedPlugins--;
            }
            
            threadedPluginStopping--;
            if (threadedPluginStopping == 0)
                stopSync.Set();
        }

        private void PluginStarted(object sender, EventArgs e)
        {
            var plugin = sender as IPlugin;
            plugin.Started -= PluginStarted;

            threadedPluginStarting--;
            if (threadedPluginStarting == 0)
                threadSync.Set();
        }

        private IEnumerable<object> InitGlobals()
        {
            var globals = globalProviders.SelectMany(gp => gp.ListGlobals()).ToList();
            foreach (var global in globals)
            {
                var name = GlobalsInfo.GetGlobalName(global);
                lua[name] = global;
            }

            return globals;
        }

        private void PrepareScriptForGlobals(IEnumerable<object> globals)
        {
            script = scriptParser.PrepareScript(script, globals);
        }

        private void RegisterEnumerations()
        {
            pluginInvoker.ListAllGlobalEnumTypes().ForEach(RegisterEnumeration);
        }

        private void RegisterEnumeration(Type type)
        {

            if (!type.IsEnum) throw new ArgumentException("The type must be an enumeration!");

            string[] names = Enum.GetNames(type);
            var values = Enum.GetValues(type);

            lua.NewTable(type.Name);
            for (int i = 0; i < names.Length; i++)
            {
                string path = type.Name + "." + names[i];
                lua[path] = values.GetValue(i);
            }
        }

        private void StopPlugins()
        {
            threadedPluginStopping = threadedPlugins;
            usedPlugins.ForEach(p => ExecuteSafe(p.Stop));
        }

        private void StopLuaEngineAndWaitUntilStopped()
        {
            running = false;
            stopSync.WaitOne();
        }

        private void WaitForThreadedPluginsToStop()
        {
            if (threadedPlugins > 0)
                stopSync.WaitOne();
        }

        private void WaitForSlowStartingPlugins()
        {
            if (pluginsAreStarting)
                threadSync.WaitOne();
        }
    }
}
