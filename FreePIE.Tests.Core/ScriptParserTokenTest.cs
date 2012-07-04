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
    public class ScriptParserTokenTest : TestBase
    {
        private const string Expression1 = @"static:test:Expression:for:parsTing";
        private const string Expression2 = @"static:test:expression(for:parsTing)";
        private const string Expression3 = "static:test:Expression:for:parsing\r\nheTj";

        [TestMethod]
        public void GetTokensFromExpression()
        {
            Register(Stub<IPluginInvoker>());

            IScriptParser parser = new LuaScriptParser(Get<IPluginInvoker>());

            parser.GetTokensFromExpression(Expression1, Expression1.IndexOf('T'))
                .AssertSequenceEqual(new[] { "static", "test", "Expression", "for", "pars" });

            parser.GetTokensFromExpression(Expression2, Expression2.IndexOf('T'))
                .AssertSequenceEqual(new [] { "for", "pars"});

            parser.GetTokensFromExpression(Expression3, Expression3.IndexOf('T'))
                .AssertSequenceEqual(new[] { "he" });
        }
    }

    public static class TestExtension
    {
        public static void AssertSequenceEqual<T>(this IEnumerable<T> listEnum, IEnumerable<T> otherEnum)
        {
            List<T> list = listEnum.ToList();
            List<T> other = otherEnum.ToList();

            if (list.Count != other.Count)
                Assert.Fail();
            else if (list.Count == 0)
                return;

            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], other[i]);
        }
    }
}
