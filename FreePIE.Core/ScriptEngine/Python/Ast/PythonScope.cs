using System.Collections.Generic;
using System.Linq;
using IronPython.Compiler.Ast;

namespace FreePIE.Core.ScriptEngine.Python.Ast
{
    public enum ScopeKind
    {
        Module,
        Class,
        Function,
        Lambda
    }

    public class PythonScope
    {
        private static readonly IList<PythonBinding> NoBindings = new List<PythonBinding>().AsReadOnly();

        private readonly Dictionary<string, List<PythonBinding>> bindings;
        private readonly HashSet<string> globalNames;
        private readonly Dictionary<string, List<PythonBinding>> attributes;

        public PythonScope(ScopeKind kind, string name, Node node, PythonScope parent)
        {
            Kind = kind;
            Name = name;
            Node = node;
            Parent = parent;
            Children = new List<PythonScope>();
            Calls = new List<PythonCall>();
            bindings = new Dictionary<string, List<PythonBinding>>();
            globalNames = new HashSet<string>();
            attributes = new Dictionary<string, List<PythonBinding>>();
            Returns = new List<Expression>();

            if (parent != null)
                parent.Children.Add(this);
        }

        public ScopeKind Kind { get; private set; }
        public string Name { get; private set; }
        public Node Node { get; private set; }
        public PythonScope Parent { get; private set; }
        public IList<PythonScope> Children { get; private set; }
        public IList<PythonCall> Calls { get; private set; }

        // Expressions returned by a function
        public IList<Expression> Returns { get; private set; }

        public FunctionDefinition Function
        {
            get { return Node as FunctionDefinition; }
        }

        public bool IsMethod
        {
            get { return Kind == ScopeKind.Function && Parent != null && Parent.Kind == ScopeKind.Class; }
        }

        public PythonScope Module
        {
            get { return Parent == null ? this : Parent.Module; }
        }

        public int BodyStartIndex
        {
            get
            {
                var function = Node as FunctionDefinition;
                if (function != null)
                    return function.Body.StartIndex;

                var @class = Node as ClassDefinition;
                if (@class != null)
                    return @class.Body.StartIndex;

                return Node.StartIndex;
            }
        }

        public IEnumerable<string> BoundNames
        {
            get { return bindings.Keys; }
        }

        public IList<PythonBinding> GetBindings(string name)
        {
            List<PythonBinding> result;
            return bindings.TryGetValue(name, out result) ? result : NoBindings;
        }

        // Instance attributes of a class (self.x = ...)
        public IEnumerable<string> AttributeNames
        {
            get { return attributes.Keys; }
        }

        public IList<PythonBinding> GetAttributes(string name)
        {
            List<PythonBinding> result;
            return attributes.TryGetValue(name, out result) ? result : NoBindings;
        }

        public void AddAttribute(PythonBinding binding)
        {
            List<PythonBinding> list;
            if (!attributes.TryGetValue(binding.Name, out list))
                attributes[binding.Name] = list = new List<PythonBinding>();

            list.Add(binding);
        }

        public void DeclareGlobal(string name)
        {
            globalNames.Add(name);
        }

        public void AddBinding(PythonBinding binding)
        {
            var scope = globalNames.Contains(binding.Name) ? Module : this;

            List<PythonBinding> list;
            if (!scope.bindings.TryGetValue(binding.Name, out list))
                scope.bindings[binding.Name] = list = new List<PythonBinding>();

            list.Add(binding);
        }

        // Python name resolution: local scope, enclosing function scopes, module.
        // Class scopes are only visible to code directly in the class body.
        public IList<PythonBinding> Lookup(string name)
        {
            var scope = this;
            var isFirst = true;

            while (scope != null)
            {
                if ((isFirst || scope.Kind != ScopeKind.Class) && scope.bindings.ContainsKey(name))
                    return scope.bindings[name];

                isFirst = false;
                scope = scope.Parent;
            }

            return NoBindings;
        }

        public IEnumerable<PythonScope> SelfAndAncestors()
        {
            for (var scope = this; scope != null; scope = scope.Parent)
                yield return scope;
        }

        public IEnumerable<PythonScope> Descendants()
        {
            return Children.SelectMany(c => new[] { c }.Concat(c.Descendants()));
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Kind, Name);
        }
    }

    public class PythonBinding
    {
        public PythonBinding(string name, Node node, Expression value, PythonScope scope, PythonScope definition)
        {
            Name = name;
            Node = node;
            Value = value;
            Scope = scope;
            Definition = definition;
        }

        public string Name { get; private set; }
        public Node Node { get; private set; }

        // The assigned expression for simple assignments (x = Foo()), otherwise null
        public Expression Value { get; private set; }

        // The scope where the binding was made
        public PythonScope Scope { get; private set; }

        // The function or class scope when the binding is a def or class statement, otherwise null
        public PythonScope Definition { get; private set; }
    }

    public class PythonCall
    {
        public PythonCall(CallExpression expression, PythonScope scope)
        {
            Expression = expression;
            Scope = scope;
        }

        public CallExpression Expression { get; private set; }
        public PythonScope Scope { get; private set; }
    }
}
