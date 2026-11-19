using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Model.Events;
using FreePIE.Core.Persistence;
using FreePIE.Core.Persistence.Paths;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;
using FreePIE.Core.ScriptEngine.Python;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace FreePIE.Tests.Core
{
    [Global(Name = "testjoystick")]
    [Documentation("A test joystick")]
    public class TestJoystickGlobal
    {
        [Documentation("The x axis")]
        public double x { get; set; }
        public double y { get; set; }

        [Documentation("Centers the stick")]
        public void center(double speed) { }

        public TestJoystickExtension extension { get; set; }
    }

    public class TestJoystickExtension
    {
        public TestStick stick { get; set; }
    }

    public class TestStick
    {
        [Documentation("Horizontal position")]
        public double x { get; set; }
    }

    [GlobalEnum]
    [Documentation("Test axes")]
    public enum TestAxis
    {
        [Documentation("The horizontal axis")]
        Horizontal = 1,
        Vertical = 2
    }

    [GlobalType(Type = typeof(TestJoystickGlobal), IsIndexed = true)]
    public class TestJoystickPlugin { }

    public class TestGlobalProvider : IGlobalProvider
    {
        public IEnumerable<object> ListGlobals()
        {
            return new object[] { new FilterHelper(), new CurveHelper(), new TestJoystickPlugin() };
        }
    }

    [TestClass]
    public class CodeCompletionTest : TestBase
    {
        private const char Caret = '|';

        private CodeCompletionProvider provider;

        [TestInitialize]
        public void Context()
        {
            var invoker = MockRepository.GenerateStub<IPluginInvoker>();
            invoker.Stub(x => x.ListAllPluginTypes()).Return(new Type[0]);
            invoker.Stub(x => x.ListAllGlobalEnumTypes()).Return(new[] { typeof(TestAxis) });

            var runtimeInfo = new RuntimeInfoProvider(invoker, new IGlobalProvider[] { new TestGlobalProvider() }, MockRepository.GenerateStub<IEventAggregator>());
            var scriptInfo = new ScriptInfoProvider(runtimeInfo, new PythonRuntimeInfo(MockRepository.GenerateStub<IPaths>())) { AnalyzeInBackground = false };

            provider = new CodeCompletionProvider(new PythonScriptParser(invoker), scriptInfo);
        }

        private static string Script(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        private IList<ExpressionInfo> Complete(params string[] lines)
        {
            var script = Script(lines);
            var offset = script.IndexOf(Caret);

            return provider.GetSuggestionsForExpression(script.Remove(offset, 1), offset).ExpressionInfos.ToList();
        }

        private IList<string> Names(params string[] lines)
        {
            return Complete(lines).Select(i => i.Name).ToList();
        }

        private SignatureHelpResult Signature(params string[] lines)
        {
            var script = Script(lines);
            var offset = script.IndexOf(Caret);

            return provider.GetSignatureHelp(script.Remove(offset, 1), offset);
        }

        private static void AssertContains(IList<string> names, params string[] expected)
        {
            foreach (var name in expected)
                Assert.IsTrue(names.Contains(name), string.Format("Expected {0} in {1}", name, string.Join(", ", names)));
        }

        private static void AssertDoesNotContain(IList<string> names, params string[] unexpected)
        {
            foreach (var name in unexpected)
                Assert.IsFalse(names.Contains(name), string.Format("Did not expect {0} in {1}", name, string.Join(", ", names)));
        }

        [TestMethod]
        public void Script_names_are_suggested_together_with_globals_builtins_and_keywords()
        {
            var names = Names(
                "speed = 10",
                "def update(value):",
                "    return value",
                "class Axis:",
                "    pass",
                "|");

            AssertContains(names, "speed", "update", "Axis", "filters", "testjoystick", "len", "range", "while", "starting");
            AssertDoesNotContain(names, "value");
        }

        [TestMethod]
        public void Names_are_filtered_by_what_has_been_written()
        {
            var names = Names(
                "speed = 10",
                "spread = 2",
                "x = sp|");

            CollectionAssert.AreEqual(new[] { "speed", "spread" }, names.ToList());
        }

        [TestMethod]
        public void Parameters_and_locals_are_only_visible_inside_the_function()
        {
            var inside = Names(
                "def update(value):",
                "    local = 1",
                "    |",
                "x = 1");

            AssertContains(inside, "value", "local", "update", "x");

            var outside = Names(
                "def update(value):",
                "    local = 1",
                "|");

            AssertDoesNotContain(outside, "value", "local");
        }

        private IList<string> ConditionNames(int lineNumber, string condition, params string[] lines)
        {
            var conditionScript = new BreakpointConditionScript(Script(lines), lineNumber);
            var offset = condition.IndexOf(Caret);
            condition = condition.Remove(offset, 1);

            return provider.GetSuggestionsForExpression(conditionScript.GetScript(condition), conditionScript.ToScriptOffset(offset))
                .ExpressionInfos.Select(i => i.Name).ToList();
        }

        [TestMethod]
        public void Breakpoint_condition_suggests_the_names_visible_at_the_breakpoint_line()
        {
            var names = ConditionNames(3, "val|",
                "def update(value):",
                "    local = 1",
                "    return local",
                "speed = 10");

            AssertContains(names, "value");

            var all = ConditionNames(3, "|",
                "def update(value):",
                "    local = 1",
                "    return local",
                "speed = 10");

            AssertContains(all, "value", "local", "update", "speed", "testjoystick");
        }

        [TestMethod]
        public void Breakpoint_condition_outside_a_function_does_not_suggest_its_locals()
        {
            var names = ConditionNames(3, "|",
                "def update(value):",
                "    return value",
                "speed = 10");

            AssertContains(names, "update", "speed");
            AssertDoesNotContain(names, "value");
        }

        [TestMethod]
        public void Breakpoint_condition_suggests_members()
        {
            var names = ConditionNames(1, "testjoystick[0].|", "x = 1");

            AssertContains(names, "x", "y", "center");
        }

        [TestMethod]
        public void Breakpoint_condition_is_placed_before_the_breakpoint_line_with_its_indentation()
        {
            var conditionScript = new BreakpointConditionScript(Script("if x:", "    y = 1"), 2);

            Assert.AreEqual(Script("if x:", "    a > 1") + "\n    y = 1", conditionScript.GetScript("a > 1"));
            Assert.AreEqual(2, conditionScript.ToConditionOffset(conditionScript.ToScriptOffset(2)));
        }

        [TestMethod]
        public void Breakpoint_condition_after_the_last_line_is_placed_at_the_end()
        {
            var conditionScript = new BreakpointConditionScript("x = 1", 5);

            Assert.AreEqual("x = 1\na", conditionScript.GetScript("a"));
        }

        [TestMethod]
        public void Kind_and_description_are_set_for_script_names()
        {
            var infos = Complete(
                "def update(value, factor=0.5):",
                "    \"\"\"Updates the value\"\"\"",
                "    return value",
                "speed = filters.simple(x, 0.5)",
                "|");

            var update = infos.Single(i => i.Name == "update");
            Assert.AreEqual(ExpressionKind.Function, update.Kind);
            StringAssert.StartsWith(update.Description, "def update(value, factor=0.5)");
            StringAssert.Contains(update.Description, "Updates the value");

            var speed = infos.Single(i => i.Name == "speed");
            Assert.AreEqual(ExpressionKind.Variable, speed.Kind);
            Assert.AreEqual("speed = filters.simple(x, 0.5)", speed.Description);

            Assert.AreEqual(ExpressionKind.Global, infos.Single(i => i.Name == "filters").Kind);
            Assert.AreEqual(ExpressionKind.Keyword, infos.Single(i => i.Name == "while").Kind);
        }

        [TestMethod]
        public void Members_of_self_includes_methods_and_instance_attributes()
        {
            var names = Names(
                "class Base:",
                "    def reset(self):",
                "        self.counter = 0",
                "class Axis(Base):",
                "    def __init__(self):",
                "        self.speed = 0",
                "    def update(self):",
                "        self.|",
                "        return 1");

            CollectionAssert.AreEqual(new[] { "counter", "reset", "speed", "update" }, names.ToList());
        }

        [TestMethod]
        public void Members_of_instances_and_attributes_are_inferred()
        {
            var names = Names(
                "class Child:",
                "    def run(self):",
                "        pass",
                "class Axis:",
                "    def __init__(self):",
                "        self.child = Child()",
                "def create():",
                "    return Axis()",
                "axis = create()",
                "axis.child.|");

            CollectionAssert.AreEqual(new[] { "run" }, names.ToList());
        }

        [TestMethod]
        public void Variables_assigned_from_globals_gets_members_of_the_global()
        {
            AssertContains(Names(
                "j = testjoystick[0]",
                "j.|"), "x", "y");

            AssertContains(Names(
                "f = filters",
                "f.s|"), "simple", "stopWatch");
        }

        [TestMethod]
        public void Variables_assigned_from_a_method_on_a_global_gets_members_of_the_returned_type()
        {
            AssertContains(Names(
                "if starting:",
                "    curve = curves.create([(0, 0), (100, 100)])",
                "filters.simple(curve.|"), "getY", "Name");

            AssertDoesNotContain(Names(
                "curve = curves.create([(0, 0), (100, 100)])",
                "curve.getY(1).|"), "getY");
        }

        [TestMethod]
        public void Added_and_deleted_curves_are_suggested_after_curves_changed()
        {
            var settings = new Settings();
            settings.AddNewCurve(Curve.Create("first", 0, 100, 3));

            var settingsManager = MockRepository.GenerateStub<ISettingsManager>();
            settingsManager.Stub(x => x.Settings).Return(settings);

            var invoker = MockRepository.GenerateStub<IPluginInvoker>();
            invoker.Stub(x => x.ListAllPluginTypes()).Return(new Type[0]);
            invoker.Stub(x => x.ListAllGlobalEnumTypes()).Return(new Type[0]);

            var eventAggregator = new EventAggregator();
            var runtimeInfo = new RuntimeInfoProvider(invoker, new IGlobalProvider[] { new CurveGlobalProvider(settingsManager) }, eventAggregator);
            Func<IList<string>> curveNames = () => runtimeInfo.AnalyzeExpression(new[] { new Token(TokenType.Identifier, string.Empty) }).Select(i => i.Name).ToList();

            CollectionAssert.AreEqual(new[] { "first" }, curveNames().ToList());

            var second = Curve.Create("second", 0, 100, 3);
            settings.AddNewCurve(second);
            eventAggregator.Publish(new CurvesChangedEvent());
            CollectionAssert.AreEqual(new[] { "first", "second" }, curveNames().ToList());

            settings.RemoveCurve(second);
            eventAggregator.Publish(new CurvesChangedEvent());
            CollectionAssert.AreEqual(new[] { "first" }, curveNames().ToList());
        }

        [TestMethod]
        public void Members_of_globals_works_as_before()
        {
            AssertContains(Names("testjoystick[0].|"), "x", "y");
            AssertContains(Names("x = filters.|"), "simple", "delta");
        }

        [TestMethod]
        public void Members_of_modules_are_suggested()
        {
            AssertContains(Names("x = math.s|"), "sin", "sqrt");
        }

        [TestMethod]
        public void Incomplete_lines_does_not_break_the_rest_of_the_script()
        {
            var names = Names(
                "def update(value):",
                "    x = filters.simple(value.|",
                "    return x",
                "speed = 1");

            Assert.AreEqual(0, names.Count);

            AssertContains(Names(
                "def update(value):",
                "    x = filters.simple(v|",
                "speed = 1"), "value");

            AssertContains(Names(
                "def update(value):",
                "    if va|",
                "        pass",
                "speed = 1"), "value");
        }

        [TestMethod]
        public void No_suggestions_in_comments_and_strings()
        {
            Assert.AreEqual(0, Names("# spe|").Count);
            Assert.AreEqual(0, Names("x = 'spe|").Count);
        }

        [TestMethod]
        public void Signature_help_for_plugin_methods_hides_the_indexer()
        {
            var result = Signature("x = filters.simple(a, |");

            Assert.AreEqual(1, result.ParameterIndex);
            Assert.AreEqual("Double simple(Double x, Double smoothing)", result.BestMatch.ToString());
        }

        [TestMethod]
        public void Documentation_attributes_are_shown_for_globals_and_their_members()
        {
            Assert.AreEqual("A test joystick", Complete("testjoy|").Single().Description);

            var members = Complete("testjoystick[0].|");
            Assert.AreEqual(ExpressionInfo.WithDocumentation("Double x", "The x axis"), members.Single(i => i.Name == "x").Description);
            Assert.AreEqual("Double y", members.Single(i => i.Name == "y").Description);
            StringAssert.EndsWith(members.Single(i => i.Name == "center").Description, "Centers the stick");
        }

        [TestMethod]
        public void Documentation_attributes_are_shown_for_nested_members()
        {
            var x = Complete("testjoystick[0].extension.stick.|").Single(i => i.Name == "x");
            Assert.AreEqual(ExpressionInfo.WithDocumentation("Double x", "Horizontal position"), x.Description);

            var assigned = Complete(
                "n = testjoystick[0].extension",
                "n.stick.|").Single(i => i.Name == "x");
            Assert.AreEqual(ExpressionInfo.WithDocumentation("Double x", "Horizontal position"), assigned.Description);
        }

        [TestMethod]
        public void Documentation_attributes_are_shown_for_global_enums_and_their_members()
        {
            Assert.AreEqual(ExpressionInfo.WithDocumentation("Enum", "Test axes"), Complete("TestAx|").Single().Description);

            var members = Complete("TestAxis.|");
            Assert.AreEqual(ExpressionInfo.WithDocumentation("Horizontal = 1", "The horizontal axis"), members.Single(i => i.Name == "Horizontal").Description);
            Assert.AreEqual("Vertical = 2", members.Single(i => i.Name == "Vertical").Description);
        }

        [TestMethod]
        public void Signature_help_shows_all_overloads_of_plugin_methods()
        {
            var result = Signature("x = filters.continuousRotation(|");

            CollectionAssert.AreEquivalent(
                new[] { "Double continuousRotation(Double x)", "Double continuousRotation(Double x, Units unit)" },
                result.Signatures.Select(s => s.ToString()).ToList());
        }

        [TestMethod]
        public void Overloaded_plugin_methods_are_suggested_once_and_describe_all_overloads()
        {
            var overloads = Complete("x = filters.continuousRot|");

            Assert.AreEqual(1, overloads.Count);
            StringAssert.Contains(overloads[0].Description, "Double continuousRotation(Double x)");
            StringAssert.Contains(overloads[0].Description, "Double continuousRotation(Double x, Units unit)");
        }

        [TestMethod]
        public void Signature_help_for_plugin_methods_shows_the_documentation()
        {
            var result = Signature("testjoystick[0].center(|");

            Assert.AreEqual("Centers the stick", result.BestMatch.Documentation);
        }

        [TestMethod]
        public void Signature_help_for_script_functions_methods_and_classes()
        {
            var function = Signature(
                "def smooth(value, factor=0.5):",
                "    return value",
                "x = smooth(|");

            Assert.AreEqual(0, function.ParameterIndex);
            CollectionAssert.AreEqual(new[] { "value", "factor=0.5" }, function.BestMatch.Parameters.ToList());

            var method = Signature(
                "class Axis:",
                "    def __init__(self, speed):",
                "        pass",
                "    def update(self, value):",
                "        pass",
                "axis = Axis(1)",
                "axis.update(foo(1, 2), |");

            Assert.AreEqual(1, method.ParameterIndex);
            CollectionAssert.AreEqual(new[] { "value" }, method.BestMatch.Parameters.ToList());

            var constructor = Signature(
                "class Axis:",
                "    def __init__(self, speed):",
                "        pass",
                "axis = Axis(|");

            CollectionAssert.AreEqual(new[] { "speed" }, constructor.BestMatch.Parameters.ToList());
        }

        [TestMethod]
        public void Script_is_analyzed_in_the_background_and_never_blocks()
        {
            var cache = new ScriptModelCache();
            var analyzed = new System.Threading.AutoResetEvent(false);
            cache.ModelUpdated += (sender, args) => analyzed.Set();

            int offset;
            var script = Script("a = 1", "x");
            cache.Update(script, script.Length);

            Assert.IsTrue(analyzed.WaitOne(5000));
            var first = cache.GetModel(script, script.Length, out offset);
            Assert.AreEqual(1, first.Module.GetBindings("a").Count);

            var changed = Script("a = 1", "b = 2", "x");
            var stale = cache.GetModel(changed, changed.Length, out offset);
            Assert.AreSame(first, stale);

            Assert.IsTrue(analyzed.WaitOne(5000));
            var updated = cache.GetModel(changed, changed.Length, out offset);
            Assert.AreNotSame(first, updated);
            Assert.AreEqual(1, updated.Module.GetBindings("b").Count);
        }

        [TestMethod]
        public void No_signature_help_outside_calls()
        {
            Assert.IsNull(Signature("x = filters.simple(a, 0.5)|"));
            Assert.IsNull(Signature("x = [filters.simple(a, 0.5), |"));
        }
    }
}
