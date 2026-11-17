using System.Collections.Generic;
using System.Linq;
using IronPython.Compiler.Ast;

namespace FreePIE.Core.ScriptEngine.Python.Ast
{
    // Semantic model of a script built from the IronPython AST.
    // Keeps track of scopes, name bindings and call sites so that we can reason about the script
    // (used by the NeedIndexer rewrite, and intended to be reused by code completion)
    public class ScriptModel
    {
        private const int MaxBaseClassDepth = 10;

        private ScriptModel(string script, PythonAst ast, bool hasErrors)
        {
            Script = script;
            Ast = ast;
            HasErrors = hasErrors;
            Module = new PythonScope(ScopeKind.Module, string.Empty, ast, null);
            Calls = new List<PythonCall>();
        }

        public string Script { get; private set; }
        public PythonAst Ast { get; private set; }
        public bool HasErrors { get; private set; }
        public PythonScope Module { get; private set; }
        public IList<PythonCall> Calls { get; private set; }

        public IEnumerable<PythonScope> Scopes
        {
            get { return new[] { Module }.Concat(Module.Descendants()); }
        }

        // Returns null if the script contains syntax errors
        public static ScriptModel Parse(string script)
        {
            var model = ParseTolerant(script);
            return model.HasErrors ? null : model;
        }

        // Builds a model from everything the parser could recover, used for scripts that are being edited
        public static ScriptModel ParseTolerant(string script)
        {
            bool hasErrors;
            var ast = PythonAstParser.Parse(script, out hasErrors);

            var model = new ScriptModel(script, ast, hasErrors);
            ast.Walk(new ScriptModelBuilder(model));

            return model;
        }

        public string GetText(Node node)
        {
            if (node.StartIndex < 0 || node.EndIndex > Script.Length || node.EndIndex < node.StartIndex)
                return string.Empty;

            return Script.Substring(node.StartIndex, node.EndIndex - node.StartIndex);
        }

        public PythonScope GetScopeAt(int index)
        {
            var scope = Module;
            PythonScope child;

            while ((child = scope.Children.FirstOrDefault(c => index >= c.BodyStartIndex && index <= c.Node.EndIndex)) != null)
                scope = child;

            return scope;
        }

        // Finds the user defined functions a call can end up in. Returns nothing if it can not be determined for sure
        public IEnumerable<PythonScope> ResolveCallTargets(PythonCall call)
        {
            var target = call.Expression.Target;

            var name = target as NameExpression;
            if (name != null)
                return GetDefinitions(call.Scope.Lookup(name.Name), ScopeKind.Function);

            var member = target as MemberExpression;
            var receiver = member != null ? member.Target as NameExpression : null;
            if (receiver == null)
                return Enumerable.Empty<PythonScope>();

            var classes = InferClassesOfVariable(receiver.Name, call.Scope).ToList();
            var methods = classes.Select(c => FindMethods(c, member.Name, 0).ToList()).ToList();

            return methods.Any(m => !m.Any())
                ? Enumerable.Empty<PythonScope>()
                : methods.SelectMany(m => m).Distinct();
        }

        public IEnumerable<PythonScope> ResolveClasses(string name, PythonScope scope)
        {
            return GetDefinitions(scope.Lookup(name), ScopeKind.Class);
        }

        // Simple type inference, a variable is considered an instance of a class if all assignments to it are calls to that class (x = Foo())
        public IEnumerable<PythonScope> InferClassesOfVariable(string name, PythonScope scope)
        {
            var self = InferSelf(name, scope);
            if (self != null)
                return new[] { self };

            var bindings = scope.Lookup(name);
            if (!bindings.Any() || bindings.Any(b => !IsClassInstantiation(b)))
                return Enumerable.Empty<PythonScope>();

            var classes = bindings
                .Select(b => ResolveClasses(((NameExpression)((CallExpression)b.Value).Target).Name, b.Scope).ToList())
                .ToList();

            return classes.Any(c => !c.Any())
                ? Enumerable.Empty<PythonScope>()
                : classes.SelectMany(c => c).Distinct();
        }

        private static bool IsClassInstantiation(PythonBinding binding)
        {
            var call = binding.Value as CallExpression;
            return call != null && call.Target is NameExpression;
        }

        // The class and its base classes that are defined in the script, closest first
        public IEnumerable<PythonScope> GetClassHierarchy(PythonScope @class)
        {
            var visited = new HashSet<PythonScope>();
            var current = new List<PythonScope> { @class };

            for (int depth = 0; depth < MaxBaseClassDepth && current.Any(); depth++)
            {
                current = current.Where(visited.Add).ToList();

                foreach (var c in current)
                    yield return c;

                current = current
                    .SelectMany(c => ((ClassDefinition)c.Node).Bases.OfType<NameExpression>().SelectMany(b => ResolveClasses(b.Name, c.Parent)))
                    .ToList();
            }
        }

        // The first parameter of a method (self) refers to an instance of the class
        public static PythonScope InferSelf(string name, PythonScope scope)
        {
            var function = scope.SelfAndAncestors().FirstOrDefault(s => s.Kind != ScopeKind.Lambda);
            if (function == null || !function.IsMethod || !function.Function.Parameters.Any())
                return null;

            var isSelf = function.Function.Parameters[0].Name == name
                         && function.GetBindings(name).Count == 1
                         && scope.Lookup(name) == function.GetBindings(name);

            return isSelf ? function.Parent : null;
        }

        private IEnumerable<PythonScope> FindMethods(PythonScope @class, string name, int depth)
        {
            var bindings = @class.GetBindings(name);
            if (bindings.Any())
                return GetDefinitions(bindings, ScopeKind.Function);

            if (depth >= MaxBaseClassDepth)
                return Enumerable.Empty<PythonScope>();

            var definition = (ClassDefinition)@class.Node;
            return definition.Bases
                .OfType<NameExpression>()
                .SelectMany(b => ResolveClasses(b.Name, @class.Parent))
                .SelectMany(b => FindMethods(b, name, depth + 1))
                .Distinct();
        }

        private static IEnumerable<PythonScope> GetDefinitions(IList<PythonBinding> bindings, ScopeKind kind)
        {
            if (!bindings.Any() || bindings.Any(b => b.Definition == null || b.Definition.Kind != kind))
                return Enumerable.Empty<PythonScope>();

            return bindings.Select(b => b.Definition).Distinct();
        }
    }
}
