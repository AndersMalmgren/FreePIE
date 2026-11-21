using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.Python;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace FreePIE.Tests.Core
{
    [TestClass]
    public class ScriptDependantPluginsTest : TestBase
    {
        private IPluginInvoker pluginInvoker;
        private IScriptParser parser;

        [TestInitialize]
        public void Context()
        {
            pluginInvoker = Stub<IPluginInvoker>();
            pluginInvoker.Stub(x => x.ListAllPluginTypes()).Return(new[] { typeof(MousePlugin), typeof(KeyboardPlugin) });
            pluginInvoker.Stub(x => x.InvokeAndConfigurePlugins(null)).IgnoreArguments().Return(Enumerable.Empty<IPlugin>());

            parser = Get<PythonScriptParser>();
        }

        private IEnumerable<Type> GetUsedPluginTypes(params string[] lines)
        {
            parser.InvokeAndConfigureAllScriptDependantPlugins(string.Join(Environment.NewLine, lines));

            return (IEnumerable<Type>)pluginInvoker.GetArgumentsForCallsMadeOn(x => x.InvokeAndConfigurePlugins(null)).Single()[0];
        }

        [TestMethod]
        public void Plugins_used_in_the_script_are_invoked()
        {
            var result = GetUsedPluginTypes(
                "def update():",
                "    mouse.deltaX = 1",
                "update()");

            CollectionAssert.AreEqual(new[] { typeof(MousePlugin) }, result.ToList());
        }

        [TestMethod]
        public void Plugins_in_comments_are_ignored()
        {
            var result = GetUsedPluginTypes(
                "#keyboard.getKeyDown(Key.A)",
                "x = 1 # keyboard",
                "'''",
                "keyboard.getKeyDown(Key.A)",
                "'''");

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void Plugins_in_strings_are_ignored()
        {
            var result = GetUsedPluginTypes(@"diagnostics.debug(""mouse"")");

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void Names_that_contain_the_name_of_a_plugin_are_ignored()
        {
            var result = GetUsedPluginTypes(
                "mousePosition = 1",
                "x.keyboard = 2");

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void Plugins_assigned_to_a_variable_with_the_same_name_are_invoked()
        {
            var result = GetUsedPluginTypes("mouse = mouse");

            CollectionAssert.AreEqual(new[] { typeof(MousePlugin) }, result.ToList());
        }

        [TestMethod]
        public void Plugins_are_found_in_scripts_with_syntax_errors()
        {
            var result = GetUsedPluginTypes(
                "keyboard.getKeyDown(Key.A)",
                "if x",
                "    mouse.deltaX = 1");

            CollectionAssert.Contains(result.ToList(), typeof(KeyboardPlugin));
        }

        [Global(Name = "mouse")]
        private class MousePlugin
        {
        }

        [Global(Name = "keyboard")]
        private class KeyboardPlugin
        {
        }
    }
}
