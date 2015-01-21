using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FreePIE.Core.Common;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model.Events;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;

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
            script = RemoveComments(script);
            var pluginTypes = pluginInvoker.ListAllPluginTypes()
                .Select(pt =>
                        new
                        {
                            Name = GlobalsInfo.GetGlobalName(pt),
                            PluginType = pt
                        }
                )
                .Where(info => script.Contains(info.Name))
                .Select(info => info.PluginType).ToList();

            return pluginInvoker.InvokeAndConfigurePlugins(pluginTypes);
        }

        private string RemoveComments(string script)
        {
            var reader = new StringReader(script);
            var result = new StringBuilder();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var commentStart = line.IndexOf("#", StringComparison.Ordinal);
                if (commentStart != -1)
                    line = line.Substring(0, commentStart);

                if(line.Length != 0)
                    result.AppendLine(line);
            }

            return result.ToString();
        }

        public IEnumerable<Type> GetAllUsedGlobalEnums(string script)
        {
            return pluginInvoker.ListAllGlobalEnumTypes()
                    .Where(t => script.Contains(t.Name))
                    .ToList();
        }

        private static readonly char[] ExpressionDelimiters = "(){}\t \r\n:".ToArray();
        private static readonly char[] ExpressionEndDelimiters = ")]\r\n\t".ToArray();
        private static readonly char[] TokenDelimiters = ".".ToArray();

        private int GetStartOfExpression(string script, int offset)
        {
            for(int i = offset; i > 0; i--)
            {
                if (ExpressionDelimiters.Contains(script[i - 1]))
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
            var globalsThatNeedsIndex = globals
                .SelectMany(g => g.GetType().GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(NeedIndexer), false).Length > 0)
                    .Select(m => new { Global = g, MethodInfo = m }))
                    .ToList();

            for (int i = 0; i < script.Length; i++)
            {
                foreach (var needIndex in globalsThatNeedsIndex)
                {
                    var name = GlobalsInfo.GetGlobalName(needIndex.Global);
                    var methodName = needIndex.MethodInfo.Name;
                    var searchFor = string.Format("{0}.{1}", name, methodName);

                    if (i + searchFor.Length <= script.Length && script.Substring(i, searchFor.Length) == searchFor)
                    {
                        int argumentStart = i + searchFor.Length;
                        var arguments = ExtractArguments(script, argumentStart);
                        int argumentEnd = argumentStart + arguments.Length;

                        var proccesedArguments = FindAndInitMethodsThatNeedIndexer(arguments.Substring(0, arguments.Length - 1), globals);

                        var newArguments = string.Format(@"{0}, ""{1}"")", proccesedArguments, arguments.Substring(1, arguments.Length - 2).Replace(@"""", @"'"));

                        script = script.Substring(0, argumentStart) +
                                    newArguments + script.Substring(argumentEnd, script.Length - argumentEnd);

                        i = argumentStart + newArguments.Length;
                    }
                }
            }

            return script;
        }

        private string ExtractArguments(string script, int start)
        {
            int parenthesesCount = 0;
            int index = start;
            do
            {
                if (script[index] == '(')
                    parenthesesCount++;

                if (script[index] == ')')
                    parenthesesCount--;

                index++;

            } while (parenthesesCount > 0 && index < script.Length);

            return script.Substring(start, index - start);
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
