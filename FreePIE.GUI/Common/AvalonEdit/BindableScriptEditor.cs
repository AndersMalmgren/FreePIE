using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Xml;
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

        private readonly BreakpointMargin breakpointMargin;
        private List<TextAnchor> breakpointAnchors = new List<TextAnchor>();
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
                if (!TextArea.LeftMargins.Contains(breakpointMargin))
                    TextArea.LeftMargins.Insert(0, breakpointMargin);
            };
        }

        private void OnDocumentChanged(object sender, EventArgs e)
        {
            if (Document != null)
                Document.Changed += OnDocumentTextChanged;

            CreateBreakpointAnchors();
        }

        public bool HasBreakpoint(int lineNumber)
        {
            return Breakpoints != null && Breakpoints.Contains(lineNumber);
        }

        public void ToggleBreakpoint(int lineNumber)
        {
            if (Breakpoints != null && !Breakpoints.Remove(lineNumber))
                Breakpoints.Add(lineNumber);
        }

        // Breakpoints follow their lines when the script is edited, anchors keep track of the lines
        private void CreateBreakpointAnchors()
        {
            breakpointAnchors = Breakpoints == null || Document == null
                ? new List<TextAnchor>()
                : Breakpoints
                    .Where(line => line >= 1 && line <= Document.LineCount)
                    .Select(line =>
                    {
                        var anchor = Document.CreateAnchor(Document.GetLineByNumber(line).Offset);
                        anchor.MovementType = AnchorMovementType.AfterInsertion;
                        return anchor;
                    })
                    .ToList();
        }

        private void OnDocumentTextChanged(object sender, DocumentChangeEventArgs e)
        {
            if (Breakpoints == null || sender != Document)
                return;

            var lines = breakpointAnchors.Where(a => !a.IsDeleted).Select(a => a.Line).Distinct().ToList();
            if (lines.SequenceEqual(Breakpoints))
                return;

            updatingBreakpoints = true;
            try
            {
                Breakpoints.Clear();
                foreach (var line in lines)
                    Breakpoints.Add(line);
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

        public ObservableCollection<int> Breakpoints
        {
            get { return (ObservableCollection<int>)GetValue(BreakpointsProperty); }
            set { SetValue(BreakpointsProperty, value); }
        }

        public static readonly DependencyProperty BreakpointsProperty = DependencyProperty.Register(
            "Breakpoints", typeof(ObservableCollection<int>), typeof(BindableScriptEditor), new PropertyMetadata(null, OnBreakpointsChanged));

        private static void OnBreakpointsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = sender as BindableScriptEditor;

            var oldBreakpoints = e.OldValue as ObservableCollection<int>;
            if (oldBreakpoints != null)
                oldBreakpoints.CollectionChanged -= editor.OnBreakpointsCollectionChanged;

            var newBreakpoints = e.NewValue as ObservableCollection<int>;
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
