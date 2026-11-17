using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FreePIE.Core.ScriptEngine.Python.Ast;
using IronPython.Compiler.Ast;

namespace FreePIE.Core.ScriptEngine.Python
{
    public class IndexerMethod
    {
        public IndexerMethod(string globalName, string methodName, string parameterName)
        {
            GlobalName = globalName;
            MethodName = methodName;
            ParameterName = parameterName;
        }

        public string GlobalName { get; private set; }
        public string MethodName { get; private set; }
        public string ParameterName { get; private set; }
    }

    // Methods marked with [NeedIndexer] keep state between script iterations and need a key that is unique for each call site.
    // Calls directly in the script get the arguments as key: filters.simple(x, 0.5) => filters.simple(x, 0.5, "x, 0.5")
    // Functions that (directly or indirectly) call such methods get a hidden parameter that carries the key of the calling site,
    // so that the same function called with different arguments gets separate state:
    //
    // def smooth(v):                                   def smooth(v, _freepie_indexer=""):
    //     return filters.simple(v, 0.5)                    return filters.simple(v, 0.5, _freepie_indexer + "v, 0.5")
    // x = smooth(joystick[0].x)                =>      x = smooth(joystick[0].x, _freepie_indexer="smooth(joystick[0].x) > ")
    // y = smooth(joystick[0].y)                        y = smooth(joystick[0].y, _freepie_indexer="smooth(joystick[0].y) > ")
    //
    // The rewrite never adds new lines so line numbers in error messages stay correct
    public class NeedIndexerRewriter
    {
        public const string IndexerParameter = "_freepie_indexer";
        private const string CallSiteSeparator = " > ";

        private readonly ScriptModel model;
        private readonly IList<IndexerMethod> indexerMethods;
        private readonly HashSet<PythonScope> functionsThatNeedIndexer;

        public NeedIndexerRewriter(ScriptModel model, IEnumerable<IndexerMethod> indexerMethods)
        {
            this.model = model;
            this.indexerMethods = indexerMethods.ToList();
            functionsThatNeedIndexer = new HashSet<PythonScope>();
        }

        public string Rewrite()
        {
            var indexerCalls = model.Calls
                .Select(c => new { Call = c, Method = FindIndexerMethod(c) })
                .Where(c => c.Method != null)
                .ToList();

            var userCalls = model.Calls
                .Where(c => !HasArgument(c.Expression, IndexerParameter))
                .Select(c => new { Call = c, Targets = model.ResolveCallTargets(c).ToList() })
                .Where(c => c.Targets.Any())
                .ToList();

            foreach (var call in indexerCalls)
                MarkNeedIndexer(call.Call.Scope);

            bool changed;
            do
            {
                changed = false;
                foreach (var call in userCalls.Where(c => c.Targets.All(functionsThatNeedIndexer.Contains)))
                    changed |= MarkNeedIndexer(call.Call.Scope);

            } while (changed);

            var edits = functionsThatNeedIndexer
                .SelectMany(AddIndexerParameter)
                .Concat(indexerCalls.SelectMany(c => PassIndexer(c.Call, c.Method)))
                .Concat(userCalls
                    .Where(c => c.Targets.All(functionsThatNeedIndexer.Contains))
                    .SelectMany(c => PassCallSiteIndexer(c.Call)))
                .ToList();

            return model.Script.ApplyEdits(edits);
        }

        private bool MarkNeedIndexer(PythonScope scope)
        {
            var owner = FindIndexerOwner(scope);
            return owner != null && functionsThatNeedIndexer.Add(owner);
        }

        // The closest function that can receive the indexer, lambdas and functions with *args are skipped
        // since they use the indexer of the enclosing function through the closure
        private static PythonScope FindIndexerOwner(PythonScope scope)
        {
            return scope.SelfAndAncestors().FirstOrDefault(CanReceiveIndexer);
        }

        private static bool CanReceiveIndexer(PythonScope scope)
        {
            if (scope.Kind != ScopeKind.Function)
                return false;

            var parameters = scope.Function.Parameters;
            return !parameters.Any(p => p.IsList || p.Name == IndexerParameter);
        }

        private IndexerMethod FindIndexerMethod(PythonCall call)
        {
            var member = call.Expression.Target as MemberExpression;
            var global = member != null ? member.Target as NameExpression : null;

            if (global == null || call.Scope.Lookup(global.Name).Any())
                return null;

            return indexerMethods.FirstOrDefault(m => m.GlobalName == global.Name && m.MethodName == member.Name);
        }

        private IEnumerable<ScriptEdit> AddIndexerParameter(PythonScope function)
        {
            var definition = function.Function;
            var parameter = string.Format(@"{0}=""""", IndexerParameter);
            var last = definition.Parameters.LastOrDefault(p => !p.IsDictionary);

            if (last != null)
            {
                var end = last.DefaultValue != null ? Math.Max(last.EndIndex, last.DefaultValue.EndIndex) : last.EndIndex;
                return new[] { new ScriptEdit(end, ", " + parameter) };
            }

            var start = model.Script.IndexOf('(', definition.StartIndex) + 1;
            return new[] { new ScriptEdit(start, definition.Parameters.Any() ? parameter + ", " : parameter) };
        }

        private IEnumerable<ScriptEdit> PassIndexer(PythonCall call, IndexerMethod method)
        {
            var value = CreateIndexerExpression(call.Scope, GetArgumentsText(call.Expression));
            var useKeyword = call.Expression.Args.Any(a => !string.IsNullOrEmpty(a.Name));

            return AppendArgument(call.Expression, useKeyword ? string.Format("{0}={1}", method.ParameterName, value) : value);
        }

        private IEnumerable<ScriptEdit> PassCallSiteIndexer(PythonCall call)
        {
            var value = CreateIndexerExpression(call.Scope, model.GetText(call.Expression) + CallSiteSeparator);
            return AppendArgument(call.Expression, string.Format("{0}={1}", IndexerParameter, value));
        }

        private string CreateIndexerExpression(PythonScope scope, string key)
        {
            var literal = ToPythonString(key);
            var owner = FindIndexerOwner(scope);

            return owner != null && functionsThatNeedIndexer.Contains(owner)
                ? string.Format("{0} + {1}", IndexerParameter, literal)
                : literal;
        }

        private IEnumerable<ScriptEdit> AppendArgument(CallExpression call, string argument)
        {
            var last = call.Args.LastOrDefault(a => a.Name != "**");
            if (last != null)
            {
                // f(x for x in y) needs parentheses around the generator when more arguments are added
                if (last.Expression is GeneratorExpression && call.Args.Count == 1)
                {
                    yield return new ScriptEdit(last.Expression.StartIndex, "(");
                    yield return new ScriptEdit(last.Expression.EndIndex, ")");
                }

                yield return new ScriptEdit(last.Expression.EndIndex, ", " + argument);
                yield break;
            }

            var dictionary = call.Args.FirstOrDefault();
            if (dictionary != null)
            {
                yield return new ScriptEdit(dictionary.StartIndex, argument + ", ");
                yield break;
            }

            yield return new ScriptEdit(GetOpeningParenthesis(call) + 1, argument);
        }

        private string GetArgumentsText(CallExpression call)
        {
            var start = GetOpeningParenthesis(call) + 1;
            var end = call.EndIndex - 1;

            return model.Script.Substring(start, end - start);
        }

        private int GetOpeningParenthesis(CallExpression call)
        {
            return model.Script.IndexOf('(', call.Target.EndIndex);
        }

        private static bool HasArgument(CallExpression call, string name)
        {
            return call.Args.Any(a => a.Name == name);
        }

        // Keys are kept on one line so that the line numbers of the script does not change
        private static string ToPythonString(string value)
        {
            value = Regex.Replace(value, @"\s*[\r\n]+\s*", " ").Replace(@"""", "'").Replace(@"\", @"\\");
            return string.Format(@"""{0}""", value);
        }
    }
}
