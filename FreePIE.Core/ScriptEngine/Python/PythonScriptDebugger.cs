using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Model.Events;
using FreePIE.Core.ScriptEngine.Python.Ast;
using IronPython.Compiler.Ast;
using IronPython.Modules;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

namespace FreePIE.Core.ScriptEngine.Python
{
    // Python tracing is only enabled while there are breakpoints or a step is in progress, otherwise the script runs at full speed.
    // IronPython tracing is per thread, so the script thread turns it on and off between executions of the script.
    // While paused the script thread waits for actions, evaluations are done on the script thread
    public class PythonScriptDebugger : IScriptDebugger
    {
        private const string ScriptFileName = "<string>";
        private const string ModuleCodeName = "<module>";
        private const string ConditionFileName = "<breakpoint condition>";
        private const int EvaluateTimeout = 1000;
        private const int MaxValueLength = 500;

        private enum StepMode
        {
            None,
            Into,
            Over,
            Out
        }

        // Conditions are compiled by the script thread the first time the breakpoint is reached
        private class ActiveBreakpoint
        {
            public ActiveBreakpoint(Breakpoint breakpoint)
            {
                Breakpoint = breakpoint;
            }

            public Breakpoint Breakpoint { get; private set; }
            public FunctionCode CompiledCondition { get; set; }
        }

        private readonly IEventAggregator eventAggregator;
        private readonly object sync = new object();
        private readonly BlockingCollection<Action> pausedActions = new BlockingCollection<Action>();
        private readonly Action resume = () => { };
        private readonly TracebackDelegate trace;

        private Microsoft.Scripting.Hosting.ScriptEngine engine;
        private string script;
        private int lineOffset;
        private int? firstStatementLine;
        private TraceBackFrame moduleStartFrame;
        private bool tracing;

        private volatile Dictionary<int, ActiveBreakpoint> breakpoints = new Dictionary<int, ActiveBreakpoint>();
        private volatile StepMode stepMode;
        private volatile bool traceChanged;
        private volatile bool stopped;
        private volatile bool paused;
        private int stepDepth;
        private TraceBackFrame pausedFrame;

        public PythonScriptDebugger(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            trace = OnTrace;
        }

        public bool IsPaused
        {
            get { return paused; }
        }

        // True when the script thread needs to call UpdateTrace, cheap enough to check before every execution of the script
        public bool TraceChanged
        {
            get { return traceChanged; }
        }

        // Called by the script thread before the script is executed the first time
        public void Attach(Microsoft.Scripting.Hosting.ScriptEngine scriptEngine, string executedScript, int scriptLineOffset)
        {
            engine = scriptEngine;
            script = executedScript;
            lineOffset = scriptLineOffset;
            traceChanged = true;
        }

        // Called by the script thread
        public void UpdateTrace()
        {
            traceChanged = false;
            SetTracing(!stopped && (breakpoints.Count > 0 || stepMode != StepMode.None));
        }

        // Must be called by the script thread before it ends, tracing that is left on slows down all scripts
        public void Detach()
        {
            traceChanged = false;
            SetTracing(false);
        }

        private void SetTracing(bool enable)
        {
            if (enable == tracing)
                return;

            tracing = enable;
            IronPython.Hosting.Python.SetTrace(engine, enable ? trace : null);
        }

        public void SetBreakpoints(IEnumerable<Breakpoint> breakpoints)
        {
            this.breakpoints = breakpoints
                .GroupBy(b => b.LineNumber)
                .ToDictionary(g => g.Key, g => new ActiveBreakpoint(g.First()));
            traceChanged = true;
        }

        public void Continue()
        {
            lock (sync)
            {
                if (!paused)
                    return;

                stepMode = StepMode.None;
                traceChanged = true;
                Resume();
            }
        }

        public void StepInto()
        {
            Step(StepMode.Into);
        }

        public void StepOver()
        {
            Step(StepMode.Over);
        }

        public void StepOut()
        {
            Step(StepMode.Out);
        }

        // Stepping while the script runs breaks at the next line that is executed
        private void Step(StepMode mode)
        {
            lock (sync)
            {
                stepMode = paused ? mode : StepMode.Into;
                traceChanged = true;
                Resume();
            }
        }

        // Releases a paused script, the debugger never pauses again
        public void Stop()
        {
            lock (sync)
            {
                stopped = true;
                traceChanged = true;
                Resume();
            }
        }

        private void Resume()
        {
            if (!paused)
                return;

            paused = false;
            pausedActions.Add(resume);
        }

        private TracebackDelegate OnTrace(TraceBackFrame frame, string result, object payload)
        {
            // Code in imported modules is not traced
            if (stopped || frame.f_code.co_filename != ScriptFileName)
                return null;

            if (result == "call" && frame.f_code.co_name == ModuleCodeName)
                OnModuleStart(frame);
            else if (result == "line")
                OnLine(frame);

            return trace;
        }

        // IronPython does not always report the first line of the module, it's already the current line when the module starts.
        // The first line is reached when the module starts if there is a statement on it
        private void OnModuleStart(TraceBackFrame frame)
        {
            if (FirstStatementLine != 1)
                return;

            moduleStartFrame = frame;
            OnLine(frame, 1);
        }

        private void OnLine(TraceBackFrame frame)
        {
            var line = Convert.ToInt32(frame.f_lineno);

            if (frame == moduleStartFrame)
            {
                moduleStartFrame = null;

                if (line == 1)
                    return;
            }

            OnLine(frame, line);
        }

        private int FirstStatementLine
        {
            get
            {
                if (!firstStatementLine.HasValue)
                {
                    var ast = PythonAstParser.Parse(script ?? string.Empty);
                    var suite = ast != null ? ast.Body as SuiteStatement : null;
                    var first = suite != null ? suite.Statements.FirstOrDefault() : ast != null ? ast.Body : null;

                    firstStatementLine = first != null ? first.Start.Line : 0;
                }

                return firstStatementLine.Value;
            }
        }

        private void OnLine(TraceBackFrame frame, int line)
        {
            var lineNumber = line - lineOffset;

            // Lines before the script are added by FreePIE
            if (lineNumber < 1)
                return;

            var mode = stepMode;
            var depth = mode == StepMode.Over || mode == StepMode.Out ? GetDepth(frame) : 0;

            ActiveBreakpoint breakpoint;

            if (mode == StepMode.Into
                || (mode == StepMode.Over && depth <= stepDepth)
                || (mode == StepMode.Out && depth < stepDepth)
                || (breakpoints.TryGetValue(lineNumber, out breakpoint) && IsConditionMet(breakpoint, frame, lineNumber)))
            {
                Pause(frame, lineNumber);
            }
        }

        // Runs inside the trace callback, IronPython does not trace code that is called from there
        private bool IsConditionMet(ActiveBreakpoint breakpoint, TraceBackFrame frame, int lineNumber)
        {
            var condition = breakpoint.Breakpoint.Condition;
            if (condition == null)
                return true;

            try
            {
                if (breakpoint.CompiledCondition == null)
                    breakpoint.CompiledCondition = (FunctionCode)Builtin.compile(DefaultContext.Default, condition, ConditionFileName, "eval", null, null);

                return PythonOps.IsTrue(Builtin.eval(DefaultContext.Default, breakpoint.CompiledCondition, (PythonDictionary)frame.f_globals, frame.f_locals));
            }
            catch (Exception e)
            {
                // Breaks like Visual Studio does, a condition with an error would otherwise silently disable the breakpoint
                eventAggregator.Publish(new ScriptErrorEvent(ErrorLevel.Warning, string.Format("Breakpoint condition '{0}' could not be evaluated: {1}", condition, e.Message), lineNumber));
                return true;
            }
        }

        private static int GetDepth(TraceBackFrame frame)
        {
            var depth = 0;

            for (var current = frame; current != null; current = current.f_back)
                depth++;

            return depth;
        }

        private void Pause(TraceBackFrame frame, int lineNumber)
        {
            lock (sync)
            {
                if (stopped)
                    return;

                pausedFrame = frame;
                stepDepth = GetDepth(frame);
                stepMode = StepMode.None;
                paused = true;
            }

            eventAggregator.Publish(new ScriptPausedEvent(lineNumber));

            Action action;
            while ((action = pausedActions.Take()) != resume)
                action();

            pausedFrame = null;
            eventAggregator.Publish(new ScriptResumedEvent());
        }

        public string Evaluate(string script, int index)
        {
            if (!paused || string.IsNullOrEmpty(script))
                return null;

            var ast = PythonAstParser.Parse(script);
            var expression = ast != null ? EvaluableExpressionFinder.Find(ast, index) : null;
            if (expression == null)
                return null;

            string value = null;
            var done = new ManualResetEventSlim();

            lock (sync)
            {
                if (!paused)
                    return null;

                pausedActions.Add(() =>
                {
                    try
                    {
                        value = Format(Evaluate(expression, pausedFrame));
                    }
                    catch (Exception)
                    {
                        // The expression can not be evaluated at the current line, for example a name that is not assigned yet
                    }
                    finally
                    {
                        done.Set();
                    }
                });
            }

            if (!done.Wait(EvaluateTimeout) || value == null)
                return null;

            return string.Format("{0} = {1}", script.Substring(expression.StartIndex, expression.EndIndex - expression.StartIndex), value);
        }

        private object Evaluate(Expression expression, TraceBackFrame frame)
        {
            var name = expression as NameExpression;
            if (name != null)
                return Lookup(name.Name, frame);

            var member = expression as MemberExpression;
            if (member != null)
                return engine.Operations.GetMember(Evaluate(member.Target, frame), member.Name);

            var index = (IndexExpression)expression;
            return PythonOps.GetIndex(DefaultContext.Default, Evaluate(index.Target, frame), ((ConstantExpression)index.Index).Value);
        }

        private static object Lookup(string name, TraceBackFrame frame)
        {
            object value;

            if (TryGetValue(frame.f_locals, name, out value)
                || TryGetValue(frame.f_globals, name, out value)
                || TryGetValue(frame.f_builtins, name, out value))
            {
                return value;
            }

            throw new KeyNotFoundException(name);
        }

        private static bool TryGetValue(object variables, string name, out object value)
        {
            value = null;
            var dictionary = variables as IDictionary<object, object>;

            return dictionary != null && dictionary.TryGetValue(name, out value);
        }

        private string Format(object value)
        {
            var text = engine.Operations.Format(value);

            return text.Length > MaxValueLength ? text.Substring(0, MaxValueLength) + "..." : text;
        }
    }
}
