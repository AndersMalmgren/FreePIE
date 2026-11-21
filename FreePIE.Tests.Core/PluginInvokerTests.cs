using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FreePIE.Core.Common;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Persistence;
using FreePIE.Core.Persistence.Paths;
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
            var settings = new Settings();
            var settingsManager = Stub<ISettingsManager>();
            settingsManager.Stub(x => x.Settings).Return(settings);
            settingsManager.Stub(x => x.GetPluginSettings(Arg<IPlugin>.Is.Anything))
                .Return(null)
                .WhenCalled(invocation => invocation.ReturnValue = settings.PluginSettings.Single(ps => ps.PluginType == invocation.Arguments[0].GetType().FullName));

            var path = typeof(TestBase).Assembly.Location;
            path = Path.GetDirectoryName(path);
            WhenCalling<IPaths>(x => x.GetApplicationPath(Arg<string>.Is.Anything)).Return(path);

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
            Assert.IsTrue(plugins.Any());
        }
    }

    [TestClass]
    public class When_finding_and_invoking_all_plugins : PluginInvokerTest
    {
        private IEnumerable<Type> pluginTypes;
        private IEnumerable<IPlugin> plugins;

        [TestInitialize]
        public void Context()
        {
            StubDllAndSettings();
            var pluginInvoker = Get<PluginInvoker>();
            pluginTypes = pluginInvoker.ListAllPluginTypes();

            // Plugins expect their properties to have been initialized with default values, which is done when the settings are loaded
            pluginInvoker.PopulatePluginSettings();
            plugins = pluginInvoker.InvokeAndConfigurePlugins(pluginTypes);
        }

        [TestMethod]
        public void It_finds_and_invokes_all_core_plugins()
        {
            Assert.IsTrue(plugins.Any());
            CollectionAssert.AreEqual(pluginTypes.ToList(), plugins.Select(p => p.GetType()).ToList());
        }
    }
}
