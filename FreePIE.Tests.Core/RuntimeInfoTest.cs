using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core
{
    [TestClass]
    public class RuntimeInfoTest : TestBase
    {
        [TestMethod]
        public void TestAnalyzeSimpleExpression()
        {

            IScriptParser parser = new LuaScriptParser(Get<IPluginInvoker>());
        }
    }
}
