using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Common;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core
{
    [TestClass]
    public class ScriptParserTokenTest : TestBase
    {
        private const string Expression1 = @"static.test:Expression:for:parsTing";
        private const string Expression2 = @"static.test:expression(for:parsTing)";
        private const string Expression3 = "static.test:Expression:for:parsing\r\nheTj";

        private static readonly Token[] Expression1Result = new []
                                                                {
                                                                    new Token(TokenType.Identifier, "static"),
                                                                    new Token(TokenType.Delimiter, "."), 
                                                                    new Token(TokenType.Identifier, "test"),
                                                                    new Token(TokenType.Delimiter, ":"), 
                                                                    new Token(TokenType.Identifier, "Expression"),
                                                                    new Token(TokenType.Delimiter, ":"), 
                                                                    new Token(TokenType.Identifier, "for"),
                                                                    new Token(TokenType.Delimiter, ":"), 
                                                                    new Token(TokenType.Identifier, "pars") 
                                                                };

        private static readonly Token[] Expression2Result = new []
                                                                {
                                                                    new Token(TokenType.Identifier, "for"),
                                                                    new Token(TokenType.Delimiter, ":"), 
                                                                    new Token(TokenType.Identifier, "pars") 
                                                                };

        private static readonly Token[] Expression3Result = new []
                                                                {
                                                                    new Token(TokenType.Identifier, "he")
                                                                };

        [TestMethod]
        public void GetTokensFromExpression()
        {
            Register(Stub<IPluginInvoker>());

            IScriptParser parser = new LuaScriptParser(Get<IPluginInvoker>());

            TestTokens(parser,
                       Expression1,
                       Expression1.IndexOf('T'),
                       Expression1Result,
                       new Range(27, 4));

            TestTokens(parser,
                       Expression2,
                       Expression2.IndexOf('T'),
                       Expression2Result,
                       new Range(27, 4));

           TestTokens(parser,
                      Expression3,
                      Expression3.IndexOf('T'),
                      Expression3Result,
                      new Range(36, 2));
        }

        public void TestTokens(IScriptParser parser, string expression, int index, Token[] resultTokens, Range range)
        {
            var result = parser.GetTokensFromExpression(expression, index);

            result.Tokens.AssertSequenceEqual(resultTokens);
            Assert.AreEqual(range.NumberOfElements, result.LastTokenRange.NumberOfElements);
            Assert.AreEqual(range.Start, result.LastTokenRange.Start);
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
