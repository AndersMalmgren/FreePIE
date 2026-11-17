using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;
using FreePIE.Core.ScriptEngine.Python;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core
{
    [TestClass]
    public class NeedIndexerTest : TestBase
    {
        private IScriptParser parser;
        private FilterHelper filters;
        private object[] globals;

        [TestInitialize]
        public void Context()
        {
            Stub<IPluginInvoker>();
            Stub<IEventAggregator>();

            parser = Get<PythonScriptParser>();
            filters = new FilterHelper();
            globals = new object[] { filters, Get<DiagnosticHelper>() };
        }

        private string Prepare(params string[] lines)
        {
            return parser.PrepareScript(Script(lines), globals);
        }

        private static string Script(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        [TestMethod]
        public void Calls_in_main_script_gets_arguments_as_indexer()
        {
            var result = Prepare(
                "x = filters.simple(a, 0.5)",
                "diagnostics.watch(x)");

            Assert.AreEqual(Script(
                @"x = filters.simple(a, 0.5, ""a, 0.5"")",
                @"diagnostics.watch(x, ""x"")"), result);
        }

        [TestMethod]
        public void Nested_calls_gets_separate_indexers()
        {
            var result = Prepare("x = filters.simple(filters.delta(a), 0.5)");

            Assert.AreEqual(@"x = filters.simple(filters.delta(a, ""a""), 0.5, ""filters.delta(a), 0.5"")", result);
        }

        [TestMethod]
        public void Comments_and_strings_are_not_rewritten()
        {
            var result = Prepare(
                "# filters.simple(a, 0.5)",
                "s = 'filters.simple(a, 0.5)'");

            Assert.AreEqual(Script(
                "# filters.simple(a, 0.5)",
                "s = 'filters.simple(a, 0.5)'"), result);
        }

        [TestMethod]
        public void Keyword_arguments_passes_indexer_as_keyword()
        {
            var result = Prepare(@"x = filters.simple(a, smoothing=0.5)");

            Assert.AreEqual(@"x = filters.simple(a, smoothing=0.5, indexer=""a, smoothing=0.5"")", result);
        }

        [TestMethod]
        public void Function_gets_indexer_from_call_site()
        {
            var result = Prepare(
                "def smooth(v):",
                "    return filters.simple(v, 0.5)",
                "x = smooth(a)",
                "y = smooth(b)");

            Assert.AreEqual(Script(
                @"def smooth(v, _freepie_indexer=""""):",
                @"    return filters.simple(v, 0.5, _freepie_indexer + ""v, 0.5"")",
                @"x = smooth(a, _freepie_indexer=""smooth(a) > "")",
                @"y = smooth(b, _freepie_indexer=""smooth(b) > "")"), result);
        }

        [TestMethod]
        public void Indexer_is_passed_through_functions_that_indirectly_need_it()
        {
            var result = Prepare(
                "def inner(v):",
                "    return filters.delta(v)",
                "def outer():",
                "    return inner(a) * 2",
                "def unrelated(v):",
                "    return v",
                "x = outer() + unrelated(a)");

            Assert.AreEqual(Script(
                @"def inner(v, _freepie_indexer=""""):",
                @"    return filters.delta(v, _freepie_indexer + ""v"")",
                @"def outer(_freepie_indexer=""""):",
                @"    return inner(a, _freepie_indexer=_freepie_indexer + ""inner(a) > "") * 2",
                @"def unrelated(v):",
                @"    return v",
                @"x = outer(_freepie_indexer=""outer() > "") + unrelated(a)"), result);
        }

        [TestMethod]
        public void Methods_gets_indexer_when_called_through_self_and_instances()
        {
            var result = Prepare(
                "class Axis:",
                "    def update(self, v):",
                "        return self.smooth(v)",
                "    def smooth(self, v):",
                "        return filters.simple(v, 0.5)",
                "axis = Axis()",
                "x = axis.update(a)");

            Assert.AreEqual(Script(
                @"class Axis:",
                @"    def update(self, v, _freepie_indexer=""""):",
                @"        return self.smooth(v, _freepie_indexer=_freepie_indexer + ""self.smooth(v) > "")",
                @"    def smooth(self, v, _freepie_indexer=""""):",
                @"        return filters.simple(v, 0.5, _freepie_indexer + ""v, 0.5"")",
                @"axis = Axis()",
                @"x = axis.update(a, _freepie_indexer=""axis.update(a) > "")"), result);
        }

        [TestMethod]
        public void Calls_that_can_not_be_resolved_to_user_functions_are_not_rewritten()
        {
            var result = Prepare(
                "def update(v):",
                "    return filters.delta(v)",
                "d = {}",
                "d.update(a)",
                "update = other",
                "update(a)");

            Assert.AreEqual(Script(
                @"def update(v, _freepie_indexer=""""):",
                @"    return filters.delta(v, _freepie_indexer + ""v"")",
                @"d = {}",
                @"d.update(a)",
                @"update = other",
                @"update(a)"), result);
        }

        [TestMethod]
        public void Functions_with_variable_arguments_uses_static_indexer()
        {
            var result = Prepare(
                "def smooth(*args):",
                "    return filters.simple(args[0], 0.5)",
                "x = smooth(a)");

            Assert.AreEqual(Script(
                @"def smooth(*args):",
                @"    return filters.simple(args[0], 0.5, ""args[0], 0.5"")",
                @"x = smooth(a)"), result);
        }

        [TestMethod]
        public void Lambdas_and_generators_uses_indexer_of_enclosing_function()
        {
            var script = parser.PrepareScript(Script(
                "def deltas(values):",
                "    f = lambda v: filters.delta(v)",
                "    return [f(v) for v in values] + list(filters.delta(v) for v in values)",
                "x = deltas(a)",
                "y = list(deltas(b) for v in a)"), globals);

            Assert.AreEqual(Script(
                @"def deltas(values, _freepie_indexer=""""):",
                @"    f = lambda v: filters.delta(v, _freepie_indexer + ""v"")",
                @"    return [f(v) for v in values] + list(filters.delta(v, _freepie_indexer + ""v"") for v in values)",
                @"x = deltas(a, _freepie_indexer=""deltas(a) > "")",
                @"y = list(deltas(b, _freepie_indexer=""deltas(b) > "") for v in a)"), script);

            var engine = IronPython.Hosting.Python.CreateEngine();
            var scope = engine.CreateScope(new Dictionary<string, object> { { "filters", filters }, { "a", new[] { 1, 2 } }, { "b", new[] { 3, 4 } } });
            engine.CreateScriptSourceFromString(script).Execute(scope);
        }

        [TestMethod]
        public void Line_numbers_are_preserved()
        {
            var script = Script(
                "def smooth(v):",
                "    return filters.simple(v,",
                "        0.5)",
                "x = smooth(",
                "    a)");

            var result = parser.PrepareScript(script, globals);

            Assert.AreEqual(script.Count(c => c == '\n'), result.Count(c => c == '\n'));
            StringAssert.Contains(result, @"_freepie_indexer=""smooth( a) > """);
        }

        [TestMethod]
        public void Script_with_syntax_errors_is_left_untouched()
        {
            var script = "x = filters.simple(a, 0.5";

            Assert.AreEqual(script, parser.PrepareScript(script, globals));
        }

        [TestMethod]
        public void Same_function_called_with_different_arguments_keeps_separate_state()
        {
            var script = parser.PrepareScript(Script(
                "def delta(v):",
                "    return filters.delta(v)",
                "x = delta(a)",
                "y = delta(b)"), globals);

            var engine = IronPython.Hosting.Python.CreateEngine();
            var compiled = engine.CreateScriptSourceFromString(script).Compile();
            var scope = engine.CreateScope(new Dictionary<string, object> { { "filters", filters } });

            scope.SetVariable("a", 1);
            scope.SetVariable("b", 100);
            compiled.Execute(scope);

            scope.SetVariable("a", 2);
            scope.SetVariable("b", 110);
            compiled.Execute(scope);

            Assert.AreEqual(1d, scope.GetVariable<double>("x"));
            Assert.AreEqual(10d, scope.GetVariable<double>("y"));
        }
    }
}
