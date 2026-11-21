using System.Collections.Generic;
using IronPython.Compiler.Ast;

namespace FreePIE.Core.ScriptEngine.Python.Ast
{
    public static class NameReferenceFinder
    {
        // Names in comments and strings are not part of the AST.
        // Bindings are not taken into account, joystick = joystick[0] still refers to the global
        public static ISet<string> Find(string script)
        {
            bool hasErrors;
            var ast = PythonAstParser.Parse(script, out hasErrors);

            var finder = new Finder();
            ast.Walk(finder);

            return finder.Names;
        }

        private class Finder : PythonWalker
        {
            public Finder()
            {
                Names = new HashSet<string>();
            }

            public HashSet<string> Names { get; private set; }

            public override bool Walk(NameExpression node)
            {
                Names.Add(node.Name);
                return false;
            }
        }
    }
}
