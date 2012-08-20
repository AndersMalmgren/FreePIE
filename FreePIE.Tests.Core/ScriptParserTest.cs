using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core
{
    [TestClass]
    public class When_parsing_a_script_with_nested_methods_that_needs_indexer : TestBase
    {
        private string script;

        [TestInitialize]
        public void Context()
        {
            Stub<IPluginInvoker>();
            var scriptParser = Get<LuaScriptParser>();

            script = scriptParser.PrepareScript("filters:delta(filters:simple(5, 0.8))", new[] {new FilterHelper()});
        }

        [TestMethod]
        public void It_should_recusivly_support_nested_methods()
        {
            Assert.AreEqual(@"filters:delta(filters:simple(5, 0.8, ""5, 0.8""), ""filters:simple(5, 0.8)"")", script);
        }
    }
}
