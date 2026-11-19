using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Xml;
using FreePIE.Core.ScriptEngine;
using FreePIE.GUI.Shells;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;

namespace FreePIE.GUI.Common.AvalonEdit
{
    public class BindableScriptEditor : TextEditor
    {
        static BindableScriptEditor()
        {
            LoadHighlightingFromManifest("FreePIE.GUI.Common.AvalonEdit.Python.xshd", "Python", ".py");
        }

        private static void LoadHighlightingFromManifest(string resource, string languageName, string fileExtension)
        {
            // Load our custom highlighting definition
            IHighlightingDefinition customHighlighting;
            using (var s = typeof(MainShellView).Assembly.GetManifestResourceStream(resource))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (var reader = new XmlTextReader(s))
                {
                    customHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting(languageName, new[] { fileExtension }, customHighlighting);
        }

        private class BreakpointAnchor
        {
            public BreakpointAnchor(TextAnchor anchor, Breakpoint breakpoint)
            {
                Anchor = anchor;
                Breakpoint = breakpoint;
            }

            public TextAnchor Anchor { get; private set; }
            public Breakpoint Breakpoint { get; private set; }
        }

        private readonly BreakpointMargin breakpointMargin;
        private List<BreakpointAnchor> breakpointAnchors = new List<BreakpointAnchor>();
        private bool updatingBreakpoints;

        public BindableScriptEditor()
        {
            TextArea.Caret.PositionChanged += CaretPositionChanged;
            SearchPanel.Install(TextArea);

            breakpointMargin = new BreakpointMargin(this);
            TextArea.TextView.BackgroundRenderers.Add(new DebuggerBackgroundRenderer(this));
            Document.Changed += OnDocumentTextChanged;
            DocumentChanged += OnDocumentChanged;

            // The breakpoint margin is placed first, the line number margin is added when ShowLineNumbers is set
            Loaded += (sender, args) =>
            {
                if (ShowBreakpointMargin && !TextArea.LeftMargins.Contains(breakpointMargin))
                    TextArea.LeftMargins.Insert(0, breakpointMargin);
            };
        }

        private void OnDocumentChanged(object sender, EventArgs e)
        {
            if (Document != null)
                Document.Changed += OnDocumentTextChanged;

            CreateBreakpointAnchors();
        }

        public bool ShowBreakpointMargin { get; set; } = true;

        public event EventHandler<BreakpointConditionRequestedEventArgs> BreakpointConditionRequested;

        public Breakpoint GetBreakpoint(int lineNumber)
        {
            return Breakpoints != null ? Breakpoints.FirstOrDefault(b => b.LineNumber == lineNumber) : null;
        }

        public bool HasBreakpoint(int lineNumber)
        {
            return GetBreakpoint(lineNumber) != null;
        }

        public void ToggleBreakpoint(int lineNumber)
        {
            if (Breakpoints == null)
                return;

            var breakpoint = GetBreakpoint(lineNumber);
            if (breakpoint != null)
                Breakpoints.Remove(breakpoint);
            else
                Breakpoints.Add(new Breakpoint(lineNumber));
        }

        public void RequestBreakpointCondition(int lineNumber)
        {
            var handler = BreakpointConditionRequested;
            if (handler != null)
                handler(this, new BreakpointConditionRequestedEventArgs(lineNumber));
        }

        // Breakpoints follow their lines when the script is edited, anchors keep track of the lines
        private void CreateBreakpointAnchors()
        {
            breakpointAnchors = Breakpoints == null || Document == null
                ? new List<BreakpointAnchor>()
                : Breakpoints
                    .Where(b => b.LineNumber >= 1 && b.LineNumber <= Document.LineCount)
                    .Select(b =>
                    {
                        var anchor = Document.CreateAnchor(Document.GetLineByNumber(b.LineNumber).Offset);
                        anchor.MovementType = AnchorMovementType.AfterInsertion;
                        return new BreakpointAnchor(anchor, b);
                    })
                    .ToList();
        }

        private void OnDocumentTextChanged(object sender, DocumentChangeEventArgs e)
        {
            if (Breakpoints == null || sender != Document)
                return;

            // Breakpoints that did not move are kept, so an unchanged collection can be detected by reference
            var moved = breakpointAnchors
                .Where(a => !a.Anchor.IsDeleted)
                .GroupBy(a => a.Anchor.Line)
                .Select(g => g.First().Breakpoint.LineNumber == g.Key ? g.First().Breakpoint : new Breakpoint(g.Key, g.First().Breakpoint.Condition))
                .ToList();

            if (moved.SequenceEqual(Breakpoints))
                return;

            updatingBreakpoints = true;
            try
            {
                Breakpoints.Clear();
                foreach (var breakpoint in moved)
                    Breakpoints.Add(breakpoint);
            }
            finally
            {
                updatingBreakpoints = false;
            }

            CreateBreakpointAnchors();
            RedrawDebuggerState();
        }

        private void OnBreakpointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!updatingBreakpoints)
                CreateBreakpointAnchors();

            RedrawDebuggerState();
        }

        private void RedrawDebuggerState()
        {
            breakpointMargin.InvalidateVisual();
            TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        }

        public ObservableCollection<Breakpoint> Breakpoints
        {
            get { return (ObservableCollection<Breakpoint>)GetValue(BreakpointsProperty); }
            set { SetValue(BreakpointsProperty, value); }
        }

        public static readonly DependencyProperty BreakpointsProperty = DependencyProperty.Register(
            "Breakpoints", typeof(ObservableCollection<Breakpoint>), typeof(BindableScriptEditor), new PropertyMetadata(null, OnBreakpointsChanged));

        private static void OnBreakpointsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = sender as BindableScriptEditor;

            var oldBreakpoints = e.OldValue as ObservableCollection<Breakpoint>;
            if (oldBreakpoints != null)
                oldBreakpoints.CollectionChanged -= editor.OnBreakpointsCollectionChanged;

            var newBreakpoints = e.NewValue as ObservableCollection<Breakpoint>;
            if (newBreakpoints != null)
                newBreakpoints.CollectionChanged += editor.OnBreakpointsCollectionChanged;

            editor.CreateBreakpointAnchors();
            editor.RedrawDebuggerState();
        }

        // The line where the script is paused, 0 when it's not paused
        public int ExecutionLine
        {
            get { return (int)GetValue(ExecutionLineProperty); }
            set { SetValue(ExecutionLineProperty, value); }
        }

        public static readonly DependencyProperty ExecutionLineProperty = DependencyProperty.Register(
            "ExecutionLine", typeof(int), typeof(BindableScriptEditor), new PropertyMetadata(0, OnExecutionLineChanged));

        private static void OnExecutionLineChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = sender as BindableScriptEditor;
            var line = (int)e.NewValue;

            if (line >= 1 && line <= editor.Document.LineCount)
                editor.ScrollToLine(line);

            editor.RedrawDebuggerState();
        }

        private void CaretPositionChanged(object sender, EventArgs e)
        {
            Caret = CaretOffset;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            Script = Text;
        }

        public static readonly DependencyProperty CaretProperty =
            DependencyProperty.Register("Caret", typeof (int), typeof (BindableScriptEditor), new PropertyMetadata(default(int), OnCaretPositionChanged));

        private static void OnCaretPositionChanged(object sender, DependencyPropertyChangedEventArgs caretChangedEventArgs)
        {
            var editor = sender as BindableScriptEditor;
            if (editor.CaretOffset != (int)caretChangedEventArgs.NewValue)
                editor.CaretOffset = (int) caretChangedEventArgs.NewValue;
        }

        public int Caret
        {
            get { return (int) GetValue(CaretProperty); }
            set { SetValue(CaretProperty, value); }
        }

        public string Script
        {
            get { return this.GetValue(ScriptProperty) as string; }
            set { this.SetValue(ScriptProperty, value); }
        }

        public Replacer Replacer
        {
            get { return this.GetValue(ReplacerProperty) as Replacer; }
            set { this.SetValue(ReplacerProperty, value); }            
        }

        public static void OnReplacerChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = sender as BindableScriptEditor;
            var replacer = e.NewValue as Replacer;
            replacer.Replace = editor.Document.Replace;
        }

        public static void OnScriptChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = sender as BindableScriptEditor;
            var newValue = e.NewValue as string;
            if (editor.Text != newValue)
                editor.Text = newValue;
        }

        public static readonly DependencyProperty ScriptProperty = DependencyProperty.Register(
          "Script", typeof(String), typeof(BindableScriptEditor), new FrameworkPropertyMetadata(string.Empty, OnScriptChanged));

        public static readonly DependencyProperty ReplacerProperty = DependencyProperty.Register(
            "Replacer", typeof(Replacer), typeof(BindableScriptEditor), new FrameworkPropertyMetadata(null, OnReplacerChanged));
    }

    public class Replacer
    {
        public Action<int, int, string> Replace { get; set; }
    }
}
