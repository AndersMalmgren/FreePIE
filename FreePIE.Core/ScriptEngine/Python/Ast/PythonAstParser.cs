using IronPython;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;

namespace FreePIE.Core.ScriptEngine.Python.Ast
{
    public static class PythonAstParser
    {
        private static readonly object engineLock = new object();
        private static readonly object parseLock = new object();
        private static Microsoft.Scripting.Hosting.ScriptEngine engine;

        private static Microsoft.Scripting.Hosting.ScriptEngine Engine
        {
            get
            {
                lock (engineLock)
                    return engine ?? (engine = IronPython.Hosting.Python.CreateEngine());
            }
        }

        public static PythonAst Parse(string script)
        {
            bool hasErrors;
            var ast = Parse(script, out hasErrors);

            return hasErrors ? null : ast;
        }

        // The parser recovers from most syntax errors, the returned AST contains everything that could be parsed
        public static PythonAst Parse(string script, out bool hasErrors)
        {
            // The parser shares the language context with other threads (script engine, code completion)
            lock (parseLock)
            {
                var source = Engine.CreateScriptSourceFromString(script, SourceCodeKind.File);
                var language = HostingHelpers.GetLanguageContext(Engine);
                var errors = new ParseErrorSink();
                var context = new CompilerContext(HostingHelpers.GetSourceUnit(source), language.GetCompilerOptions(), errors);
                var parser = Parser.CreateParser(context, (PythonOptions)language.Options);

                var ast = parser.ParseFile(false);

                hasErrors = errors.HasErrors;
                return ast;
            }
        }

        private class ParseErrorSink : ErrorSink
        {
            public bool HasErrors { get; private set; }

            public override void Add(SourceUnit source, string message, SourceSpan span, int errorCode, Severity severity)
            {
                if (severity == Severity.Error || severity == Severity.FatalError)
                    HasErrors = true;
            }
        }
    }
}
