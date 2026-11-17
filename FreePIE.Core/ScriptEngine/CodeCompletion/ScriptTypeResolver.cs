using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FreePIE.Core.ScriptEngine.Python;
using FreePIE.Core.ScriptEngine.Python.Ast;
using IronPython.Compiler.Ast;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    // Simple type inference over the script model, enough to know what members to suggest after a dot
    internal class ScriptTypeResolver
    {
        private const int MaxDepth = 10;
        private const int MaxValueLength = 80;
        private const string ImplicitModule = "math";
        private const string Constructor = "__init__";

        private readonly ScriptModel model;
        private readonly IRuntimeInfoProvider runtimeInfoProvider;
        private readonly IPythonRuntimeInfo pythonRuntimeInfo;

        public ScriptTypeResolver(ScriptModel model, IRuntimeInfoProvider runtimeInfoProvider, IPythonRuntimeInfo pythonRuntimeInfo)
        {
            this.model = model;
            this.runtimeInfoProvider = runtimeInfoProvider;
            this.pythonRuntimeInfo = pythonRuntimeInfo;
        }

        public IRuntimeInfoProvider RuntimeInfoProvider
        {
            get { return runtimeInfoProvider; }
        }

        public IPythonRuntimeInfo PythonRuntimeInfo
        {
            get { return pythonRuntimeInfo; }
        }

        public ScriptModel Model
        {
            get { return model; }
        }

        // Names visible from the scope, using the same rules as Python (class scopes are only visible directly in the class body)
        public IEnumerable<ExpressionInfo> GetVisibleNames(PythonScope scope, Token partial)
        {
            var names = new Dictionary<string, ExpressionInfo>();
            var isFirst = true;

            for (var current = scope; current != null; current = current.Parent)
            {
                if (isFirst || current.Kind != ScopeKind.Class)
                {
                    foreach (var name in current.BoundNames.Where(n => IsVisibleName(n) && IsPartialMatch(n, partial) && !names.ContainsKey(n)))
                        names[name] = CreateInfo(name, current.GetBindings(name), false);
                }

                isFirst = false;
            }

            return names.Values;
        }

        internal static bool IsPartialMatch(string name, Token partial)
        {
            return name.StartsWith(partial.Value, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsVisibleName(string name)
        {
            return !name.StartsWith("<") && !name.Contains("$") && name != NeedIndexerRewriter.IndexerParameter;
        }

        // Resolves a dotted path like self.axis or joystick[0], identifiers can be indexed (name[index])
        public ScriptType ResolvePath(PythonScope scope, IList<string> path)
        {
            var type = ResolveToken(path[0], t => ResolveName(t, scope, 0));

            foreach (var member in path.Skip(1))
            {
                if (type == null)
                    return null;

                var parent = type;
                type = ResolveToken(member, t => parent.GetMember(t));
            }

            return type;
        }

        private static ScriptType ResolveToken(string token, Func<string, ScriptType> resolve)
        {
            var bracket = token.IndexOf('[');
            if (bracket < 0)
                return resolve(token);

            var alias = resolve(token.Substring(0, bracket)) as AliasType;
            return alias != null ? alias.Indexed(token.Substring(bracket)) : null;
        }

        public ScriptType ResolveName(string name, PythonScope scope, int depth)
        {
            var self = ScriptModel.InferSelf(name, scope);
            if (self != null)
                return new ClassType(this, self, true);

            var bindings = scope.Lookup(name);
            if (!bindings.Any())
                return name == ImplicitModule ? (ScriptType)new ModuleType(this, name) : new AliasType(this, new[] { new Token(TokenType.Identifier, name) });

            return InferBindings(bindings, depth);
        }

        public ScriptType InferBindings(IEnumerable<PythonBinding> bindings, int depth)
        {
            if (depth > MaxDepth)
                return null;

            return bindings.Select(b => InferBinding(b, depth)).FirstOrDefault(t => t != null);
        }

        private ScriptType InferBinding(PythonBinding binding, int depth)
        {
            if (binding.Definition != null)
                return binding.Definition.Kind == ScopeKind.Class ? new ClassType(this, binding.Definition, false) : null;

            var module = GetImportedModule(binding);
            if (module != null)
                return new ModuleType(this, module);

            return binding.Value != null ? InferExpression(binding.Value, binding.Scope, depth + 1) : null;
        }

        public ScriptType InferExpression(Expression expression, PythonScope scope, int depth)
        {
            if (depth > MaxDepth)
                return null;

            var name = expression as NameExpression;
            if (name != null)
                return ResolveName(name.Name, scope, depth + 1);

            var parenthesis = expression as ParenthesisExpression;
            if (parenthesis != null)
                return InferExpression(parenthesis.Expression, scope, depth + 1);

            var member = expression as MemberExpression;
            if (member != null)
            {
                var target = InferExpression(member.Target, scope, depth + 1);
                return target != null ? target.GetMember(member.Name) : null;
            }

            var index = expression as IndexExpression;
            if (index != null)
            {
                var alias = InferExpression(index.Target, scope, depth + 1) as AliasType;
                return alias != null ? alias.Indexed(string.Format("[{0}]", model.GetText(index.Index))) : null;
            }

            var call = expression as CallExpression;
            if (call != null)
                return InferCall(call, scope, depth + 1);

            return null;
        }

        private ScriptType InferCall(CallExpression call, PythonScope scope, int depth)
        {
            // The runtime info has the members of the returned type under the method of a global, so curves.create(...) resolves like curves.create
            var member = call.Target as MemberExpression;
            var global = member != null ? InferExpression(member.Target, scope, depth + 1) as AliasType : null;
            if (global != null)
                return global.GetMember(member.Name);

            var target = InferCallTarget(call.Target, scope, depth);
            if (target == null)
                return null;

            if (target.Kind == ScopeKind.Class)
                return new ClassType(this, target, true);

            return target.Returns
                .Select(r => InferExpression(r, model.GetScopeAt(r.StartIndex), depth + 1))
                .FirstOrDefault(t => t != null);
        }

        private PythonScope InferCallTarget(Expression target, PythonScope scope, int depth)
        {
            var name = target as NameExpression;
            if (name != null)
                return scope.Lookup(name.Name).Select(b => b.Definition).FirstOrDefault(d => d != null);

            var member = target as MemberExpression;
            if (member == null)
                return null;

            var type = InferExpression(member.Target, scope, depth + 1) as ClassType;
            return type != null ? type.FindDefinition(member.Name) : null;
        }

        private static string GetImportedModule(PythonBinding binding)
        {
            var import = binding.Node as ImportStatement;
            if (import != null)
            {
                for (int i = 0; i < import.Names.Count; i++)
                {
                    var names = import.Names[i].Names;
                    var asName = import.AsNames != null ? import.AsNames[i] : null;

                    if (asName == binding.Name)
                        return string.Join(".", names);

                    if (asName == null && names.First() == binding.Name)
                        return names.First();
                }
            }

            var fromImport = binding.Node as FromImportStatement;
            if (fromImport != null)
            {
                for (int i = 0; i < fromImport.Names.Count; i++)
                {
                    var asName = fromImport.AsNames != null ? fromImport.AsNames[i] : null;
                    if ((asName ?? fromImport.Names[i]) == binding.Name)
                        return string.Format("{0}.{1}", string.Join(".", fromImport.Root.Names), fromImport.Names[i]);
                }
            }

            return null;
        }

        public ExpressionInfo CreateInfo(string name, IList<PythonBinding> bindings, bool isBoundMember)
        {
            var definition = bindings.Select(b => b.Definition).FirstOrDefault(d => d != null);

            if (definition != null && definition.Kind == ScopeKind.Function)
                return CreateFunctionInfo(name, definition, isBoundMember);

            if (definition != null && definition.Kind == ScopeKind.Class)
                return CreateClassInfo(name, definition);

            var module = bindings.Select(GetImportedModule).FirstOrDefault(m => m != null);
            if (module != null)
                return new ExpressionInfo { Name = name, Kind = ExpressionKind.Module, Description = string.Format("module {0}", module) };

            var parameter = bindings.Select(b => b.Node).OfType<Parameter>().FirstOrDefault();
            if (parameter != null)
                return new ExpressionInfo { Name = name, Kind = ExpressionKind.Parameter, Description = string.Format("parameter {0}", FormatParameter(parameter)) };

            var isField = bindings.Any(b => b.Node is MemberExpression || b.Scope.Kind == ScopeKind.Class);
            var value = bindings.Where(b => b.Value != null).Select(b => FormatValue(b.Value)).FirstOrDefault();

            return new ExpressionInfo
            {
                Name = name,
                Kind = isField ? ExpressionKind.Field : ExpressionKind.Variable,
                Description = value != null ? string.Format("{0} = {1}", name, value) : string.Format("{0} {1}", isField ? "field" : "variable", name)
            };
        }

        private ExpressionInfo CreateFunctionInfo(string name, PythonScope function, bool isBoundMember)
        {
            var definition = function.Function;
            var parameters = definition.Parameters.Select(FormatParameter).ToList();
            var signatureParameters = isBoundMember && function.IsMethod ? parameters.Skip(1) : parameters;
            var header = string.Format("def {0}({1})", name, string.Join(", ", parameters));

            return new ExpressionInfo
            {
                Name = name,
                Kind = function.IsMethod ? ExpressionKind.Method : ExpressionKind.Function,
                Description = ExpressionInfo.WithDocumentation(header, GetDocumentation(definition.Body)),
                Signature = new SignatureInfo(name, signatureParameters, "def", GetDocumentation(definition.Body))
            };
        }

        private ExpressionInfo CreateClassInfo(string name, PythonScope @class)
        {
            var definition = (ClassDefinition)@class.Node;
            var bases = definition.Bases.Select(model.GetText).ToList();
            var header = bases.Any() ? string.Format("class {0}({1})", name, string.Join(", ", bases)) : string.Format("class {0}", name);

            var constructor = model.GetClassHierarchy(@class)
                .Select(c => c.GetBindings(Constructor).Select(b => b.Definition).FirstOrDefault(d => d != null && d.Kind == ScopeKind.Function))
                .FirstOrDefault(d => d != null);

            var parameters = constructor != null ? constructor.Function.Parameters.Skip(1).Select(FormatParameter) : Enumerable.Empty<string>();

            return new ExpressionInfo
            {
                Name = name,
                Kind = ExpressionKind.Class,
                Description = ExpressionInfo.WithDocumentation(header, GetDocumentation(definition.Body)),
                Signature = new SignatureInfo(name, parameters, "class", GetDocumentation(definition.Body))
            };
        }

        private string FormatParameter(Parameter parameter)
        {
            var prefix = parameter.IsList ? "*" : parameter.IsDictionary ? "**" : string.Empty;
            var defaultValue = parameter.DefaultValue != null ? "=" + model.GetText(parameter.DefaultValue) : string.Empty;

            return prefix + parameter.Name + defaultValue;
        }

        private string FormatValue(Expression value)
        {
            var text = Regex.Replace(model.GetText(value), @"\s+", " ");
            return text.Length > MaxValueLength ? text.Substring(0, MaxValueLength) + "..." : text;
        }

        // The documentation property of the AST is only set when the script is compiled
        private static string GetDocumentation(Statement body)
        {
            var suite = body as SuiteStatement;
            var first = suite != null ? suite.Statements.FirstOrDefault() : body;
            var expression = first as ExpressionStatement;
            var constant = expression != null ? expression.Expression as ConstantExpression : null;

            return constant != null ? constant.Value as string : null;
        }
    }

    internal abstract class ScriptType
    {
        protected ScriptType(ScriptTypeResolver resolver)
        {
            Resolver = resolver;
        }

        protected ScriptTypeResolver Resolver { get; private set; }

        public abstract IEnumerable<ExpressionInfo> GetMembers(Token partial);
        public abstract ScriptType GetMember(string name);
    }

    // A class defined in the script, either the class itself or an instance of it
    internal class ClassType : ScriptType
    {
        private readonly PythonScope @class;
        private readonly bool isInstance;

        public ClassType(ScriptTypeResolver resolver, PythonScope @class, bool isInstance) : base(resolver)
        {
            this.@class = @class;
            this.isInstance = isInstance;
        }

        private IEnumerable<PythonScope> Hierarchy
        {
            get { return Resolver.Model.GetClassHierarchy(@class); }
        }

        public override IEnumerable<ExpressionInfo> GetMembers(Token partial)
        {
            var showPrivate = partial.Value.StartsWith("_");
            var members = new Dictionary<string, ExpressionInfo>();

            foreach (var c in Hierarchy)
            {
                foreach (var name in c.BoundNames.Where(n => ScriptTypeResolver.IsPartialMatch(n, partial) && !members.ContainsKey(n)))
                    members[name] = Resolver.CreateInfo(name, c.GetBindings(name), isInstance);

                if (!isInstance)
                    continue;

                foreach (var name in c.AttributeNames.Where(n => ScriptTypeResolver.IsPartialMatch(n, partial) && !members.ContainsKey(n)))
                    members[name] = Resolver.CreateInfo(name, c.GetAttributes(name), true);
            }

            return members.Values
                .Where(i => showPrivate || !i.Name.StartsWith("__"))
                .OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase);
        }

        public override ScriptType GetMember(string name)
        {
            foreach (var c in Hierarchy)
            {
                var bindings = c.GetBindings(name);
                if (bindings.Any())
                    return Resolver.InferBindings(bindings, 0);

                var attributes = isInstance ? c.GetAttributes(name) : null;
                if (attributes != null && attributes.Any())
                    return Resolver.InferBindings(attributes, 0);
            }

            return null;
        }

        public PythonScope FindDefinition(string name)
        {
            return Hierarchy
                .Select(c => c.GetBindings(name).Select(b => b.Definition).FirstOrDefault(d => d != null))
                .FirstOrDefault(d => d != null);
        }
    }

    // An expression that refers to the FreePIE plugins and globals, resolved with the runtime info tree (joystick[0], filters)
    internal class AliasType : ScriptType
    {
        private readonly IList<Token> tokens;

        public AliasType(ScriptTypeResolver resolver, IEnumerable<Token> tokens) : base(resolver)
        {
            this.tokens = tokens.ToList();
        }

        public AliasType Indexed(string index)
        {
            var indexed = tokens.ToList();
            var last = indexed.Last();
            indexed[indexed.Count - 1] = new Token(last.Type, last.Value + index);

            return new AliasType(Resolver, indexed);
        }

        public override IEnumerable<ExpressionInfo> GetMembers(Token partial)
        {
            return Resolver.RuntimeInfoProvider.AnalyzeExpression(tokens.Concat(new[] { new Token(TokenType.Delimiter, "."), partial }));
        }

        public override ScriptType GetMember(string name)
        {
            return new AliasType(Resolver, tokens.Concat(new[] { new Token(TokenType.Delimiter, "."), new Token(TokenType.Identifier, name) }));
        }
    }

    // A Python module (math, time ...)
    internal class ModuleType : ScriptType
    {
        private readonly string module;

        public ModuleType(ScriptTypeResolver resolver, string module) : base(resolver)
        {
            this.module = module;
        }

        public override IEnumerable<ExpressionInfo> GetMembers(Token partial)
        {
            var members = Resolver.PythonRuntimeInfo.GetModuleMembers(module) ?? Enumerable.Empty<ExpressionInfo>();
            return members.Where(i => ScriptInfoProvider.IsPartialMatch(i, partial));
        }

        public override ScriptType GetMember(string name)
        {
            var member = (Resolver.PythonRuntimeInfo.GetModuleMembers(module) ?? Enumerable.Empty<ExpressionInfo>())
                .FirstOrDefault(i => i.Name == name);

            return member != null && member.Kind == ExpressionKind.Module ? new ModuleType(Resolver, string.Format("{0}.{1}", module, name)) : null;
        }
    }
}
