using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Common;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model.Events;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;
using FreePIE.Core.ScriptEngine.Python.Ast;

namespace FreePIE.Core.ScriptEngine.Python
{
    public class PythonScriptParser : IScriptParser
    {
        private readonly IPluginInvoker pluginInvoker;

        public PythonScriptParser(IPluginInvoker pluginInvoker)
        {
            this.pluginInvoker = pluginInvoker;
        }

        public IEnumerable<IPlugin> InvokeAndConfigureAllScriptDependantPlugins(string script)
        {
            var names = NameReferenceFinder.Find(script);
            var pluginTypes = pluginInvoker.ListAllPluginTypes()
                .Where(pt => names.Contains(GlobalsInfo.GetGlobalName(pt)))
                .ToList();

            return pluginInvoker.InvokeAndConfigurePlugins(pluginTypes);
        }

        public IEnumerable<Type> GetAllUsedGlobalEnums(string script)
        {
            return pluginInvoker.ListAllGlobalEnumTypes()
                    .Where(t => script.Contains(t.Name))
                    .ToList();
        }

        private static readonly char[] ExpressionDelimiters = "(){}\t \r\n:=+-*/%<>!,;&|^~@'\"#".ToArray();
        private static readonly char[] ExpressionEndDelimiters = ")]}\r\n\t :=+-*/%<>!,;&|^~@'\"#".ToArray();
        private static readonly char[] TokenDelimiters = ".".ToArray();

        // Indexers are part of the token (joystick[0].x) so delimiters inside brackets are ignored
        private int GetStartOfExpression(string script, int offset)
        {
            int depth = 0;
            for(int i = offset; i > 0; i--)
            {
                var c = script[i - 1];

                if (c == ']')
                    depth++;
                else if (c == '[')
                {
                    if (depth == 0)
                    {
                        // An indexer being written (joystick[0) is part of the token
                        var isIndexer = i > 1 && char.IsLetterOrDigit(script[i - 2]) && script.Substring(i, offset - i).All(char.IsDigit);
                        if (!isIndexer)
                            return i;
                    }
                    else
                        depth--;
                }
                else if (depth == 0 && ExpressionDelimiters.Contains(c))
                    return i;
            }

            return 0;
        }

        private IEnumerable<Token> ExtractTokens(StringBuilder currentToken, char delimiter)
        {
            yield return new Token(TokenType.Identifier, currentToken.Extract());
            yield return new Token(TokenType.Delimiter, delimiter.ToString());
        }

        private Token ExtractToken(StringBuilder currentToken)
        {
            return new Token(TokenType.Identifier, currentToken.Extract());
        }

        public TokenResult GetTokensFromExpression(string script, int offset)
        {
            var tokens = new List<Token>();

            int start = GetStartOfExpression(script, offset);

            var token = new StringBuilder();

            for(int i = start; i < offset; i++)
            {
                if(!TokenDelimiters.Contains(script[i]))
                    token.Append(script[i]);
                else 
                {
                    tokens.AddRange(ExtractTokens(token, script[i]));
                }
            }

            tokens.Add(ExtractToken(token));

            Token lastToken = tokens.Last();

            return new TokenResult(tokens, new Range(offset - lastToken.Value.Length, lastToken.Value.Length));
        }

        public string PrepareScript(string script, IEnumerable<object> globals)
        {
            return FindAndInitMethodsThatNeedIndexer(script, globals);
        }

        public bool IsEndOfExpressionDelimiter(char @char)
        {
            return ExpressionEndDelimiters.Contains(@char);
        }

        public IEnumerable<ScriptErrorEvent> ListDeprecatedWarnings(string script, IEnumerable<object> globals)
        {
            return globals
                .SelectMany(g => g.GetType().GetMembers() 
                    .Select(m => new { Atr = m.GetCustomAttributes(typeof(Deprecated), false).FirstOrDefault() as Deprecated, m.Name })
                    .Where(m => m.Atr != null)
                    .Select(m => new { m.Atr.ReplacedWith, Deprecated = string.Format("{0}.{1}", GlobalsInfo.GetGlobalName(g), m.Name) })
                )
                .GroupBy(m => m.Deprecated)
                .Select(m => new { Info = m.First(), IndexOf = script.IndexOf(m.Key, StringComparison.Ordinal)})
                .Where(m => m.IndexOf >= 0)
                .Select(m => new ScriptErrorEvent(ErrorLevel.Warning, string.Format("{0} marked as deprecated, use {1}", m.Info.Deprecated, m.Info.ReplacedWith), GetLineNumber(script, m.IndexOf)))
                .ToList();
        }

        private int GetLineNumber(string script, int indexOf)
        {
            return script
                .Substring(0, indexOf)
                .Split(Environment.NewLine.ToArray())
                .Count(s => s != string.Empty) + 1;
        }

        private string FindAndInitMethodsThatNeedIndexer(string script, IEnumerable<object> globals)
        {
            var methodsThatNeedIndexer = globals
                .SelectMany(g => g.GetType().GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(NeedIndexer), false).Length > 0)
                    .Select(m => new IndexerMethod(GlobalsInfo.GetGlobalName(g), m.Name, m.GetParameters().Last().Name)))
                .GroupBy(m => new { m.GlobalName, m.MethodName })
                .Select(g => g.First())
                .ToList();

            if (!methodsThatNeedIndexer.Any())
                return script;

            var model = ScriptModel.Parse(script);

            // Syntax errors are reported by the script engine when the script is compiled
            if (model == null)
                return script;

            return new NeedIndexerRewriter(model, methodsThatNeedIndexer).Rewrite();
        }
    }

    internal static class StringBuilderExtensions
    {
        public static string Extract(this StringBuilder builder)
        {
            string retVal = builder.ToString();

            builder.Length = 0;

            return retVal;
        }
    }
}
