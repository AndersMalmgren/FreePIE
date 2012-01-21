using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Common;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Persistence;
using FreePIE.Core.Plugins;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace FreePIE.Tests.Core
{
    public abstract class PluginInvokerTest : TestBase
    {
        protected void StubDllAndSettings()
        {
            WhenCalling<ISettingsManager>(x => x.GetPluginSettings(Arg<IOPlugin>.Is.Anything)).Return(new PluginSetting());
            var path = typeof(TestBase).Assembly.Location;
            path = Path.GetDirectoryName(path);

            var dlls = Directory.GetFiles(path, "*plugins.dll");
            WhenCalling<IFileSystem>(x => x.GetFiles(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(dlls);
        }
    }


    [TestClass]
    public class When_listing_all_core_plugin_types : PluginInvokerTest
    {
        private IEnumerable<Type> plugins;

        [TestInitialize]
        public void Context()
        {
            
            StubDllAndSettings();
            var pluginInvoker = Get<PluginInvoker>();
            plugins = pluginInvoker.ListAllPluginTypes();
        }

        [TestMethod]
        public void It_finds_all_core_plugin_types()
        {
            Assert.AreEqual(1, plugins.Count());
        }
    }

    [TestClass]
    public class When_finding_and_invoking_all_plugins : PluginInvokerTest
    {
        private IEnumerable<IOPlugin> plugins;

        [TestInitialize]
        public void Context()
        {
            StubDllAndSettings();
            var pluginInvoker = Get<PluginInvoker>();
            plugins = pluginInvoker.InvokeAndConfigurePlugins(pluginInvoker.ListAllPluginTypes());
        }

        [TestMethod]
        public void It_finds_and_invokes_all_core_plugins()
        {
            Assert.AreEqual(1, plugins.Count());
        }
    }
}
