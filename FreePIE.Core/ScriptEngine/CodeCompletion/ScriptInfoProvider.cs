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

        // Starts analyzing the script in the background so that suggestions are ready when needed
        void ScriptChanged(string script, int offset);

        // Raised on a background thread when the script has been analyzed again, suggestions may have changed
        event EventHandler ScriptAnalyzed;
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

        private readonly ScriptModelCache modelCache;

        public ScriptInfoProvider(IRuntimeInfoProvider runtimeInfoProvider, IPythonRuntimeInfo pythonRuntimeInfo)
        {
            this.runtimeInfoProvider = runtimeInfoProvider;
            this.pythonRuntimeInfo = pythonRuntimeInfo;
            modelCache = new ScriptModelCache();
            modelCache.ModelUpdated += (sender, args) => OnScriptAnalyzed();

            ThreadPool.QueueUserWorkItem(x => WarmUp());
        }

        public event EventHandler ScriptAnalyzed;

        // Parse on the calling thread instead of in the background, used by tests
        public bool AnalyzeInBackground
        {
            get { return modelCache.AnalyzeInBackground; }
            set { modelCache.AnalyzeInBackground = value; }
        }

        private void OnScriptAnalyzed()
        {
            var handler = ScriptAnalyzed;
            if (handler != null)
                handler(this, EventArgs.Empty);
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

        public void ScriptChanged(string script, int offset)
        {
            script = script ?? string.Empty;
            modelCache.Update(script, Math.Max(0, Math.Min(offset, script.Length)));
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
            var model = modelCache.GetModel(script, offset, out modelOffset);
            var resolver = new ScriptTypeResolver(model, runtimeInfoProvider, pythonRuntimeInfo);
            var scope = model.GetScopeAt(modelOffset);

            if (!path.Any())
                return resolver.GetVisibleNames(scope, partial)
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
    }
}
