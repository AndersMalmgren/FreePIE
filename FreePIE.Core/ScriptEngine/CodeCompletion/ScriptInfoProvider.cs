using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FreePIE.Core.ScriptEngine.Python;
using FreePIE.Core.ScriptEngine.Python.Ast;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public interface IScriptInfoProvider
    {
        // Suggestions for the expression ending at offset, the last token is the one being written
        IEnumerable<ExpressionInfo> AnalyzeExpression(string script, int offset, IList<Token> tokens);
    }

    // Combines information from the script itself (variables, functions, classes) with
    // the plugins and globals from the runtime info provider and the Python builtins
    public class ScriptInfoProvider : IScriptInfoProvider
    {
        private const string ImplicitModule = "math";

        private static readonly ExpressionInfo[] EngineGlobals =
        {
            new ExpressionInfo { Name = "starting", Kind = ExpressionKind.Variable, Description = "bool starting\n\nTrue the first time the script is executed" },
            new ExpressionInfo { Name = "stopping", Kind = ExpressionKind.Variable, Description = "bool stopping\n\nTrue the last time the script is executed, when the script is stopped" }
        };

        private readonly IRuntimeInfoProvider runtimeInfoProvider;
        private readonly IPythonRuntimeInfo pythonRuntimeInfo;

        private string cachedScript;
        private int cachedLineStart = -1;
        private ScriptModel cachedModel;

        public ScriptInfoProvider(IRuntimeInfoProvider runtimeInfoProvider, IPythonRuntimeInfo pythonRuntimeInfo)
        {
            this.runtimeInfoProvider = runtimeInfoProvider;
            this.pythonRuntimeInfo = pythonRuntimeInfo;

            ThreadPool.QueueUserWorkItem(x => WarmUp());
        }

        // Creating the Python engine takes a while, do it before the user starts typing
        private void WarmUp()
        {
            try
            {
                ScriptModel.ParseTolerant(string.Empty);
                pythonRuntimeInfo.Builtins.Count();
                pythonRuntimeInfo.GetModuleMembers(ImplicitModule);
            }
            catch (Exception)
            { }
        }

        public IEnumerable<ExpressionInfo> AnalyzeExpression(string script, int offset, IList<Token> tokens)
        {
            script = script ?? string.Empty;
            offset = Math.Max(0, Math.Min(offset, script.Length));

            var partial = tokens.Last();
            var path = tokens
                .Take(tokens.Count - 1)
                .Where(t => t.Type == TokenType.Identifier)
                .Select(t => t.Value)
                .ToList();

            int modelOffset;
            var model = GetModel(script, offset, out modelOffset);
            var resolver = new ScriptTypeResolver(model, runtimeInfoProvider, pythonRuntimeInfo);
            var scope = model.GetScopeAt(modelOffset);

            if (!path.Any())
                return resolver.GetVisibleNames(scope).Where(i => IsPartialMatch(i, partial))
                    .Concat(EngineGlobals.Where(i => IsPartialMatch(i, partial)))
                    .Concat(runtimeInfoProvider.AnalyzeExpression(tokens))
                    .Concat(pythonRuntimeInfo.Builtins.Where(i => IsPartialMatch(i, partial)))
                    .Concat(pythonRuntimeInfo.Keywords.Where(i => IsPartialMatch(i, partial)))
                    .ToList();

            var type = resolver.ResolvePath(scope, path);

            return type != null
                ? type.GetMembers(partial).ToList()
                : runtimeInfoProvider.AnalyzeExpression(tokens).ToList();
        }

        internal static bool IsPartialMatch(ExpressionInfo info, Token partial)
        {
            return info.Name.StartsWith(partial.Value, StringComparison.InvariantCultureIgnoreCase);
        }

        // The line being edited is usually not valid Python, it's replaced with a statement of the same indentation
        // so that the rest of the script parses and the scope of the line is kept
        private ScriptModel GetModel(string script, int offset, out int modelOffset)
        {
            var lineStart = ScriptTextHelper.GetLineStart(script, offset);
            var lineEnd = ScriptTextHelper.GetLineEnd(script, offset);
            var line = script.Substring(lineStart, lineEnd - lineStart);
            var indent = new string(line.TakeWhile(char.IsWhiteSpace).ToArray());

            modelOffset = lineStart + indent.Length;

            if (cachedModel != null && cachedScript == script && cachedLineStart == lineStart)
                return cachedModel;

            var isBlockHeader = ScriptTextHelper.StripComment(line).TrimEnd().EndsWith(":");
            var replacement = indent + (isBlockHeader ? "if 1:" : "pass");

            cachedModel = ScriptModel.ParseTolerant(script.Substring(0, lineStart) + replacement + script.Substring(lineEnd));
            cachedScript = script;
            cachedLineStart = lineStart;

            return cachedModel;
        }
    }
}
