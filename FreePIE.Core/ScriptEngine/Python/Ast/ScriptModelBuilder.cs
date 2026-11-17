using System.Collections.Generic;
using System.Linq;
using IronPython.Compiler.Ast;

namespace FreePIE.Core.ScriptEngine.Python.Ast
{
    internal class ScriptModelBuilder : PythonWalker
    {
        private readonly ScriptModel model;
        private readonly Stack<PythonScope> scopes;

        public ScriptModelBuilder(ScriptModel model)
        {
            this.model = model;
            scopes = new Stack<PythonScope>();
            scopes.Push(model.Module);
        }

        private PythonScope Current
        {
            get { return scopes.Peek(); }
        }

        public override bool Walk(FunctionDefinition node)
        {
            // Lambdas and generator expressions are represented as anonymous functions (<lambda$1>, <genexpr>)
            var isAnonymous = node.IsLambda || node.Name.StartsWith("<");
            var scope = new PythonScope(isAnonymous ? ScopeKind.Lambda : ScopeKind.Function, node.Name, node, Current);

            if (!isAnonymous)
                Current.AddBinding(new PythonBinding(node.Name, node, null, Current, scope));

            foreach (var parameter in node.Parameters)
                scope.AddBinding(new PythonBinding(parameter.Name, parameter, null, scope, null));

            scopes.Push(scope);
            return true;
        }

        public override void PostWalk(FunctionDefinition node)
        {
            scopes.Pop();
        }

        public override bool Walk(ClassDefinition node)
        {
            var scope = new PythonScope(ScopeKind.Class, node.Name, node, Current);
            Current.AddBinding(new PythonBinding(node.Name, node, null, Current, scope));

            scopes.Push(scope);
            return true;
        }

        public override void PostWalk(ClassDefinition node)
        {
            scopes.Pop();
        }

        public override bool Walk(CallExpression node)
        {
            var call = new PythonCall(node, ScopeOf(node));
            call.Scope.Calls.Add(call);
            model.Calls.Add(call);

            return true;
        }

        public override bool Walk(AssignmentStatement node)
        {
            foreach (var target in node.Left)
                Bind(target, node.Right);

            return true;
        }

        public override bool Walk(AugmentedAssignStatement node)
        {
            Bind(node.Left, null);
            return true;
        }

        public override bool Walk(ReturnStatement node)
        {
            var function = Current.SelfAndAncestors().FirstOrDefault(s => s.Kind == ScopeKind.Function);
            if (function != null && node.Expression != null)
                function.Returns.Add(node.Expression);

            return true;
        }

        public override bool Walk(ForStatement node)
        {
            Bind(node.Left, null);
            return true;
        }

        public override bool Walk(ComprehensionFor node)
        {
            Bind(node.Left, null);
            return true;
        }

        public override bool Walk(WithStatement node)
        {
            if (node.Variable != null)
                Bind(node.Variable, null);

            return true;
        }

        public override bool Walk(TryStatementHandler node)
        {
            if (node.Target != null)
                Bind(node.Target, null);

            return true;
        }

        public override bool Walk(ImportStatement node)
        {
            for (int i = 0; i < node.Names.Count; i++)
            {
                var asName = node.AsNames != null ? node.AsNames[i] : null;
                BindName(asName ?? node.Names[i].Names.First(), node);
            }

            return true;
        }

        public override bool Walk(FromImportStatement node)
        {
            for (int i = 0; i < node.Names.Count; i++)
            {
                if (node.Names[i] == "*")
                    continue;

                var asName = node.AsNames != null ? node.AsNames[i] : null;
                BindName(asName ?? node.Names[i], node);
            }

            return true;
        }

        public override bool Walk(GlobalStatement node)
        {
            foreach (var name in node.Names)
                Current.DeclareGlobal(name);

            return true;
        }

        private void Bind(Expression target, Expression value)
        {
            var name = target as NameExpression;
            if (name != null)
            {
                Current.AddBinding(new PythonBinding(name.Name, target, value, Current, null));
                return;
            }

            var member = target as MemberExpression;
            if (member != null)
            {
                BindAttribute(member, value);
                return;
            }

            var parenthesis = target as ParenthesisExpression;
            if (parenthesis != null)
            {
                Bind(parenthesis.Expression, value);
                return;
            }

            var sequence = target as SequenceExpression;
            if (sequence != null)
            {
                foreach (var item in sequence.Items)
                    Bind(item, null);
            }
        }

        // self.x = value inside a method adds an instance attribute to the class
        private void BindAttribute(MemberExpression target, Expression value)
        {
            var receiver = target.Target as NameExpression;
            if (receiver == null)
                return;

            var @class = ScriptModel.InferSelf(receiver.Name, Current);
            if (@class != null)
                @class.AddAttribute(new PythonBinding(target.Name, target, value, Current, null));
        }

        private void BindName(string name, Node node)
        {
            Current.AddBinding(new PythonBinding(name, node, null, Current, null));
        }

        // Default values, decorators and base classes are evaluated in the enclosing scope
        private PythonScope ScopeOf(Node node)
        {
            var scope = Current;
            while (scope.Kind != ScopeKind.Module && node.StartIndex < scope.BodyStartIndex)
                scope = scope.Parent;

            return scope;
        }
    }
}
