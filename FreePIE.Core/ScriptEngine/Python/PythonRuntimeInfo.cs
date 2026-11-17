using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FreePIE.Core.Persistence.Paths;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;

namespace FreePIE.Core.ScriptEngine.Python
{
    public interface IPythonRuntimeInfo
    {
        IEnumerable<ExpressionInfo> Builtins { get; }
        IEnumerable<ExpressionInfo> Keywords { get; }

        // Returns null if the module can not be imported
        IEnumerable<ExpressionInfo> GetModuleMembers(string module);
    }

    // Information about the Python runtime (builtins and modules) for code completion
    public class PythonRuntimeInfo : IPythonRuntimeInfo
    {
        private const int MaxDocumentationLines = 15;

        private static readonly string[] PythonKeywords =
        {
            "and", "as", "assert", "break", "class", "continue", "def", "del", "elif", "else", "except", "exec", "finally", "for",
            "from", "global", "if", "import", "in", "is", "lambda", "not", "or", "pass", "print", "raise", "return", "try", "while",
            "with", "yield", "None", "True", "False"
        };

        private static readonly Regex SignaturePattern = new Regex(@"^\s*(\w+)\((.*)\)");

        private readonly IPaths paths;
        private readonly object engineLock = new object();
        private readonly Dictionary<string, IList<ExpressionInfo>> modules;
        private Microsoft.Scripting.Hosting.ScriptEngine engine;
        private IList<ExpressionInfo> builtins;

        public PythonRuntimeInfo(IPaths paths)
        {
            this.paths = paths;
            modules = new Dictionary<string, IList<ExpressionInfo>>();
        }

        private Microsoft.Scripting.Hosting.ScriptEngine Engine
        {
            get
            {
                if (engine == null)
                {
                    engine = IronPython.Hosting.Python.CreateEngine();

                    var pylib = paths != null ? paths.GetApplicationPath("pylib") : null;
                    if (!string.IsNullOrEmpty(pylib))
                        engine.SetSearchPaths(new[] { pylib });
                }

                return engine;
            }
        }

        public IEnumerable<ExpressionInfo> Keywords
        {
            get { return PythonKeywords.Select(k => new ExpressionInfo { Name = k, Description = "Keyword", Kind = ExpressionKind.Keyword }); }
        }

        public IEnumerable<ExpressionInfo> Builtins
        {
            get
            {
                lock (engineLock)
                    return builtins ?? (builtins = MapMembers(IronPython.Hosting.Python.GetBuiltinModule(Engine)));
            }
        }

        public IEnumerable<ExpressionInfo> GetModuleMembers(string module)
        {
            lock (engineLock)
            {
                IList<ExpressionInfo> members;
                if (modules.TryGetValue(module, out members))
                    return members;

                try
                {
                    members = MapMembers(IronPython.Hosting.Python.ImportModule(Engine, module));
                }
                catch (Exception)
                {
                    members = null;
                }

                modules[module] = members;
                return members;
            }
        }

        private IList<ExpressionInfo> MapMembers(ScriptScope scope)
        {
            return scope.GetVariableNames()
                .Where(name => !name.StartsWith("_"))
                .Select(name => MapMember(name, (object)scope.GetVariable(name)))
                .OrderBy(info => info.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private ExpressionInfo MapMember(string name, object value)
        {
            var documentation = GetDocumentation(value);
            var info = new ExpressionInfo { Name = name, Kind = GetKind(value) };

            switch (info.Kind)
            {
                case ExpressionKind.Function:
                case ExpressionKind.Class:
                    info.Description = documentation;
                    info.Signature = ParseSignature(name, documentation);
                    break;
                case ExpressionKind.Module:
                    info.Description = string.Format("module {0}", name);
                    break;
                default:
                    info.Description = string.Format("{0} = {1}", name, value);
                    break;
            }

            return info;
        }

        private ExpressionKind GetKind(object value)
        {
            if (value is PythonType)
                return ExpressionKind.Class;

            if (value is PythonModule)
                return ExpressionKind.Module;

            return Engine.Operations.IsCallable(value) ? ExpressionKind.Function : ExpressionKind.Variable;
        }

        private string GetDocumentation(object value)
        {
            object doc;
            if (!Engine.Operations.TryGetMember(value, "__doc__", out doc) || !(doc is string))
                return string.Empty;

            return string.Join(Environment.NewLine, ((string)doc).Trim().Split('\n').Take(MaxDocumentationLines).Select(l => l.TrimEnd()));
        }

        // Python documentation usually starts with the signature, for example "sin(x)"
        private static SignatureInfo ParseSignature(string name, string documentation)
        {
            var match = SignaturePattern.Match(documentation);
            if (!match.Success || match.Groups[1].Value != name)
                return null;

            var parameters = match.Groups[2].Value
                .Split(',')
                .Select(p => p.Trim())
                .Where(p => p.Length > 0);

            return new SignatureInfo(name, parameters, string.Empty, documentation);
        }
    }
}
