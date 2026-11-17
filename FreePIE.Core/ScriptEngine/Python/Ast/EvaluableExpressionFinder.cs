using IronPython.Compiler.Ast;

namespace FreePIE.Core.ScriptEngine.Python.Ast
{
    // Finds the expression at a position in the script that can be evaluated without side effects:
    // names, members and indexing with constants (joystick[0].x). Calls are never evaluated
    public static class EvaluableExpressionFinder
    {
        public static Expression Find(PythonAst ast, int index)
        {
            var finder = new Finder(index);
            ast.Walk(finder);

            return finder.Result != null && IsEvaluable(finder.Result) ? finder.Result : null;
        }

        public static bool IsEvaluable(Expression expression)
        {
            if (expression is NameExpression)
                return true;

            var member = expression as MemberExpression;
            if (member != null)
                return IsEvaluable(member.Target);

            var index = expression as IndexExpression;
            if (index != null)
                return index.Index is ConstantExpression && IsEvaluable(index.Target);

            return false;
        }

        private class Finder : PythonWalker
        {
            private readonly int index;

            public Finder(int index)
            {
                this.index = index;
            }

            public Expression Result { get; private set; }

            public override bool Walk(NameExpression node)
            {
                if (Contains(node.StartIndex, node.EndIndex))
                    Result = node;

                return false;
            }

            // Only the member name selects the member expression (x in joystick[0].x), the target is walked as usual
            public override bool Walk(MemberExpression node)
            {
                if (!Contains(node.EndIndex - node.Name.Length, node.EndIndex))
                    return true;

                Result = node;
                return false;
            }

            private bool Contains(int start, int end)
            {
                return index >= start && index < end;
            }
        }
    }
}
