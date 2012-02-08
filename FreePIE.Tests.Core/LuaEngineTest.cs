using System;
using System.ComponentModel;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FreePIE.Core.Common;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Persistence;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace FreePIE.Tests.Core
{
    [TestClass]
    public class When_initilizing_Lua_engine : TestBase
    {

        private string callbackValue = "ping";
        private string actualCallbackValue;
        private Dictionary<string, object> dummyCallbacks = new Dictionary<string, object>();

        [TestInitialize]
        public void Context()
        {
            Action<string> callback = s => actualCallbackValue = s;
            var plugin = new Plugin(callback);
            var plugins = new List<IPlugin> {plugin};


        var points = new List<Point> {new Point(0, 0), new Point(1, 2), new Point(2, 2.5), new Point(3, 4)};
            var settings = new Settings
            {
                Curves = new List<Curve> { new Curve { Name="testCurve", Points = points} },
            };

            var globalDummy = new GlobalDummy((key, value) =>
            {
                dummyCallbacks[key] = value;
            });

            Stub<ISettingsManager>().Expect(x => x.Settings).Return(settings);
            WhenCalling<IPluginInvoker>(x => x.InvokeAndConfigurePlugins(Arg<IEnumerable<Type>>.Is.Anything)).Return(plugins);
            WhenCalling<IPluginInvoker>(x => x.ListAllPluginTypes()).Return(plugins.Select(p => p.GetType()));
            Register<IGlobalProvider>(Get<ScriptHelpersGlobalProvider>());
            Register<IGlobalProvider>(Get<CurveGlobalProvider>());
            Register<IScriptParser>(Get<LuaScriptParser>());
            WhenCallingNewInstance<IGlobalProvider>(x => x.ListGlobals()).Return(new List<object> {globalDummy});
            
            var engine = Get<LuaEngine>();
            const string simpleScript = @"
if(starting) then
    x = 0.1;
end

y = testCurve:getY(x)
x = y
diagnostics:debug(y)
z = 1
filters:simple(z, 0.5)
z = 0.5
z = filters:simple(z, 0.5)
if(starting) then
    globalDummy:callback(""testIndexParser"", z)
end
filters:simple(testCurve:getY(x), 0.5) --Test more complex parsing for NeedIndex algorithm
--This needs to be last to test the other two
testPlugin:dummy(""ping"")
";

            engine.Start(simpleScript);

            //Wait for the script to run atleast once
            Thread.Sleep(50);

            engine.Stop();
        }

        public interface IMyGlobal
        {
            void Callaback(string callbackValue);
        }

        [TestMethod]
        public void It_should_load_all_globals_and_init_startup_correctly()
        {
            Assert.AreEqual(callbackValue, actualCallbackValue);
            Assert.AreEqual(dummyCallbacks["testIndexParser"], 0.75);
        }
    }

    [LuaGlobalType(Type = typeof(PluginGlobal))]
    public class Plugin : IPlugin
    {
        private readonly Action<string> action;
        public bool Crash { get; set; }
        public double TestValue { get; set; }
        private bool running;
        

        public Plugin(Action<string> action)
        {
            this.action = action;
        }

        public object CreateGlobal()
        {
            return new PluginGlobal(action, this);
        }

        public Action Start()
        {
            Crash = true;

            return () =>
            {
                Crash = false;
                running = true;

                Started(this, new EventArgs());
                while(running) {}
            };
        }

        public void Stop()
        {
            running = false;
        }

        public event EventHandler Started;

        public string FriendlyName
        {
            get { return "Plugin"; }
        }

        public bool GetProperty(int index, IPluginProperty property)
        {
            return false;
        }

        public bool SetProperties(Dictionary<string, object> properties)
        {
            return false;
        }
        
        

        public void DoBeforeNextExecute()
        {
        }
    }


    [LuaGlobal(Name = "testPlugin")]
    public class PluginGlobal
    {
        private Action<string> action;
        private readonly Plugin plugin;

        public PluginGlobal(Action<string> action, Plugin plugin)
        {
            this.action = action;
            this.plugin = plugin;
        }

        public void dummy(string text)
        {
            if (plugin.Crash)
                throw new Exception("Thread handling in lua engine broken");

            action(text);
        }
    }

    [LuaGlobal(Name = "globalDummy")]
    public class GlobalDummy
    {
        private readonly Action<string, object> action;

        public GlobalDummy(Action<string, object> action)
        {
            this.action = action;
        }

        public void callback(string key, object value)
        {
            action(key, value);
        }
    }
}
