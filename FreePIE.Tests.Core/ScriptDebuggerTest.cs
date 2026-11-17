using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Model.Events;
using FreePIE.Core.ScriptEngine.Python;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core
{
    [TestClass]
    public class ScriptDebuggerTest : IHandle<ScriptPausedEvent>
    {
        private const int Timeout = 5000;

        private ScriptEngine engine;
        private PythonScriptDebugger debugger;
        private BlockingCollection<int> pausedLines;
        private Thread thread;
        private string script;
        private ScriptScope scope;

        [TestInitialize]
        public void Context()
        {
            engine = IronPython.Hosting.Python.CreateEngine();
            scope = engine.CreateScope();
            pausedLines = new BlockingCollection<int>();

            var eventAggregator = new EventAggregator();
            eventAggregator.Subscribe(this);
            debugger = new PythonScriptDebugger(eventAggregator);
        }

        [TestCleanup]
        public void Cleanup()
        {
            debugger.Stop();
            if (thread != null)
                Assert.IsTrue(thread.Join(Timeout), "Script thread did not stop");
        }

        public void Handle(ScriptPausedEvent message)
        {
            pausedLines.Add(message.LineNumber);
        }

        private static string Script(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        // Runs the script on its own thread the same way as the script engine does
        private void Run(int iterations, params string[] lines)
        {
            script = Script(lines);
            var compiled = engine.CreateScriptSourceFromString(script).Compile();

            thread = new Thread(() =>
            {
                debugger.Attach(engine, script, 0);
                try
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        if (debugger.TraceChanged)
                            debugger.UpdateTrace();

                        compiled.Execute(scope);
                    }
                }
                finally
                {
                    debugger.Detach();
                }

                // Tracing must be off for the thread when the debugger is detached
                compiled.Execute(scope);
            });

            thread.Start();
        }

        private int NextPausedLine()
        {
            int line;
            Assert.IsTrue(pausedLines.TryTake(out line, Timeout), "Script did not pause");
            return line;
        }

        private void WaitForScript()
        {
            Assert.IsTrue(thread.Join(Timeout), "Script did not finish");
            thread = null;
        }

        private string EvaluateAt(string text, int occurrence = 0)
        {
            var index = -1;
            for (var i = 0; i <= occurrence; i++)
                index = script.IndexOf(text, index + 1, StringComparison.Ordinal);

            return debugger.Evaluate(script, index);
        }

        private object Variable(string name)
        {
            return scope.GetVariable(name);
        }

        private static readonly string[] FunctionScript =
        {
            "def f(a):",
            "    b = a * 2",
            "    return b",
            "x = f(3)",
            "y = x + 1"
        };

        // Records if the script is traced, once for every execution. The last execution is done after the debugger is detached
        private static readonly string[] TraceRecordingScript =
        {
            "import sys",
            "traced.append(sys.gettrace() is not None)"
        };

        private bool[] Traced()
        {
            return ((IronPython.Runtime.List)Variable("traced")).Cast<bool>().ToArray();
        }

        [TestMethod]
        public void Script_is_not_traced_without_breakpoints()
        {
            scope.SetVariable("traced", new IronPython.Runtime.List());
            Run(2, TraceRecordingScript);
            WaitForScript();

            CollectionAssert.AreEqual(new[] { false, false, false }, Traced());
        }

        [TestMethod]
        public void Script_is_traced_while_there_are_breakpoints()
        {
            scope.SetVariable("traced", new IronPython.Runtime.List());
            debugger.SetBreakpoints(new[] { 100 });
            Run(2, TraceRecordingScript);
            WaitForScript();

            CollectionAssert.AreEqual(new[] { true, true, false }, Traced());
        }

        [TestMethod]
        public void Tracing_is_turned_off_when_breakpoints_are_removed()
        {
            scope.SetVariable("traced", new IronPython.Runtime.List());
            debugger.SetBreakpoints(new[] { 1 });
            Run(2, TraceRecordingScript);

            Assert.AreEqual(1, NextPausedLine());
            debugger.SetBreakpoints(new int[0]);
            debugger.Continue();
            WaitForScript();

            CollectionAssert.AreEqual(new[] { true, false, false }, Traced());
        }

        [TestMethod]
        public void Pauses_at_breakpoint_and_evaluates_locals()
        {
            debugger.SetBreakpoints(new[] { 3 });
            Run(1, FunctionScript);

            Assert.AreEqual(3, NextPausedLine());
            Assert.IsTrue(debugger.IsPaused);
            Assert.AreEqual("b = 6", EvaluateAt("b", 1));
            Assert.AreEqual("a = 3", EvaluateAt("a", 1));

            debugger.Continue();
            WaitForScript();

            Assert.AreEqual(7, Variable("y"));
        }

        [TestMethod]
        public void Pauses_at_breakpoint_every_execution()
        {
            debugger.SetBreakpoints(new[] { 1 });
            Run(2, "x = 1");

            Assert.AreEqual(1, NextPausedLine());
            debugger.Continue();
            Assert.AreEqual(1, NextPausedLine());
            debugger.Continue();
            WaitForScript();
        }

        [TestMethod]
        public void Pauses_once_at_first_line()
        {
            debugger.SetBreakpoints(new[] { 1 });
            Run(1, "import math", "x = 1");

            Assert.AreEqual(1, NextPausedLine());
            debugger.Continue();
            WaitForScript();

            Assert.AreEqual(0, pausedLines.Count);
        }

        [TestMethod]
        public void Pauses_at_breakpoint_in_function_every_execution()
        {
            debugger.SetBreakpoints(new[] { 2 });
            Run(3, FunctionScript);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(2, NextPausedLine());
                debugger.Continue();
            }

            WaitForScript();
        }

        [TestMethod]
        public void Step_over_does_not_enter_functions()
        {
            debugger.SetBreakpoints(new[] { 4 });
            Run(1, FunctionScript);

            Assert.AreEqual(4, NextPausedLine());
            debugger.StepOver();
            Assert.AreEqual(5, NextPausedLine());
        }

        [TestMethod]
        public void Step_into_enters_functions()
        {
            debugger.SetBreakpoints(new[] { 4 });
            Run(1, FunctionScript);

            Assert.AreEqual(4, NextPausedLine());
            debugger.StepInto();
            Assert.AreEqual(2, NextPausedLine());
            debugger.StepOver();
            Assert.AreEqual(3, NextPausedLine());
        }

        [TestMethod]
        public void Step_out_returns_to_caller()
        {
            debugger.SetBreakpoints(new[] { 2 });
            Run(1, FunctionScript);

            Assert.AreEqual(2, NextPausedLine());
            debugger.StepOut();
            Assert.AreEqual(5, NextPausedLine());
        }

        [TestMethod]
        public void Step_over_last_line_pauses_at_next_execution()
        {
            debugger.SetBreakpoints(new[] { 2 });
            Run(2, "x = 1", "y = 2");

            Assert.AreEqual(2, NextPausedLine());
            debugger.SetBreakpoints(new int[0]);
            debugger.StepOver();
            Assert.AreEqual(1, NextPausedLine());
        }

        [TestMethod]
        public void Step_while_running_pauses_at_next_line()
        {
            debugger.StepInto();
            Run(1, "x = 1", "y = 2");

            Assert.AreEqual(1, NextPausedLine());
        }

        [TestMethod]
        public void Lines_added_before_the_script_are_not_part_of_it()
        {
            script = Script("import math", "", "x = 1");
            var compiled = engine.CreateScriptSourceFromString(script).Compile();
            debugger.StepInto();

            thread = new Thread(() =>
            {
                debugger.Attach(engine, script, 2);
                try
                {
                    debugger.UpdateTrace();
                    compiled.Execute(scope);
                }
                finally
                {
                    debugger.Detach();
                }
            });
            thread.Start();

            Assert.AreEqual(1, NextPausedLine());
        }

        [TestMethod]
        public void Evaluates_members_and_constant_indexes()
        {
            debugger.SetBreakpoints(new[] { 6 });
            Run(1,
                "class A(object):",
                "    def __init__(self):",
                "        self.v = [1, 2]",
                "a = A()",
                "l = [a]",
                "w = l[0].v[1]");

            Assert.AreEqual(6, NextPausedLine());
            Assert.AreEqual("l[0].v = [1, 2]", EvaluateAt("v[1]"));
            Assert.AreEqual("l = [<A object at", EvaluateAt("l[0]").Substring(0, 17));
        }

        [TestMethod]
        public void Evaluates_self_members_in_methods()
        {
            debugger.SetBreakpoints(new[] { 4 });
            Run(1,
                "class A(object):",
                "    def __init__(self):",
                "        self.v = 5",
                "        self.w = self.v",
                "a = A()");

            Assert.AreEqual(4, NextPausedLine());
            Assert.AreEqual("self.v = 5", EvaluateAt("v", 1));
        }

        [TestMethod]
        public void Calls_are_not_evaluated()
        {
            debugger.SetBreakpoints(new[] { 3 });
            Run(1,
                "def f():",
                "    return 1",
                "x = f().real");

            Assert.AreEqual(3, NextPausedLine());
            Assert.IsNull(EvaluateAt("real"));
            Assert.IsNotNull(EvaluateAt("f()", 1));
        }

        [TestMethod]
        public void Names_that_are_not_assigned_yet_are_not_evaluated()
        {
            debugger.SetBreakpoints(new[] { 1 });
            Run(1, "x = 1");

            Assert.AreEqual(1, NextPausedLine());
            Assert.IsNull(EvaluateAt("x"));
        }

        [TestMethod]
        public void Nothing_is_evaluated_while_running()
        {
            Run(1, "x = 1");
            WaitForScript();

            Assert.IsNull(EvaluateAt("x"));
        }

        [TestMethod]
        public void Stop_releases_a_paused_script()
        {
            debugger.SetBreakpoints(new[] { 1 });
            Run(10, "x = 1");

            Assert.AreEqual(1, NextPausedLine());
            debugger.Stop();
            WaitForScript();

            Assert.IsFalse(debugger.IsPaused);
        }
    }
}
