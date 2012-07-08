using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Common;
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

            TestTokens(parser,
                       Expression1,
                       Expression1.IndexOf('T'),
                       new[] { "static", "test", "Expression", "for", "pars" },
                       new Range(27, 4));

            TestTokens(parser,
                       Expression2,
                       Expression2.IndexOf('T'),
                       new [] { "for", "pars"},
                       new Range(27, 4));

           TestTokens(parser,
                      Expression3,
                      Expression3.IndexOf('T'),
                      new[] { "he" },
                      new Range(36, 2));
            
            parser.GetTokensFromExpression(Expression3, Expression3.IndexOf('T')).Tokens
                .AssertSequenceEqual(new[] { "he" });
        }

        public void TestTokens(IScriptParser parser, string expression, int index, string[] resultTokens, Range range)
        {
            var result = parser.GetTokensFromExpression(expression, index);

            result.Tokens.AssertSequenceEqual(resultTokens);
            Assert.AreEqual(range.NumberOfElements, result.LastToken.NumberOfElements);
            Assert.AreEqual(range.Start, result.LastToken.Start);
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
