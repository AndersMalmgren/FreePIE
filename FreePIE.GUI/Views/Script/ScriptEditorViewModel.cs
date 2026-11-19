using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Events;
using FreePIE.Core.ScriptEngine;
using FreePIE.GUI.CodeCompletion;
using FreePIE.GUI.Common.AvalonEdit;
using FreePIE.GUI.Common.CodeCompletion;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells;
using FreePIE.GUI.Views.Main;

namespace FreePIE.GUI.Views.Script
{
    public class ScriptEditorViewModel : PanelViewModel, IHandle<ScriptStateChangedEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ICodeCompletionProvider provider;
        private readonly FileSystem fileSystem;
        private readonly IResultFactory resultFactory;

        public ScriptEditorViewModel(IEventAggregator eventAggregator, ICodeCompletionProvider provider, CompletionPopupViewModel completionModel, FileSystem fileSystem, IResultFactory resultFactory)
        {
            this.eventAggregator = eventAggregator;
            this.provider = provider;
            this.fileSystem = fileSystem;
            this.resultFactory = resultFactory;
            Replacer = new Replacer();
            CompletionWindow = completionModel;
            SignatureHelp = new SignatureHelpViewModel();
            Breakpoints = new Caliburn.Micro.BindableCollection<Breakpoint>();
            Breakpoints.CollectionChanged += OnBreakpointsChanged;
            eventAggregator.Subscribe(this);
            provider.ScriptAnalyzed += (sender, args) => Caliburn.Micro.Execute.BeginOnUIThread(OnScriptAnalyzed);
            completion = new ScriptCompletion(provider, completionModel, () => script, () => caretPosition, OnInsertion);
        }

        private readonly ScriptCompletion completion;
        
        private static int untitledIndex;
        private int untitledId;

        public ScriptEditorViewModel Configure(string filePath)
        {
            FilePath = filePath;
            if (string.IsNullOrEmpty(filePath))
            {
                untitledId = untitledIndex++;
            }

            return this;
        }

        private void OnScriptAnalyzed()
        {
            completion.ScriptAnalyzed();
            UpdateSignatureHelp();
        }

        public CompletionPopupViewModel CompletionWindow { get; set; }

        public SignatureHelpViewModel SignatureHelp { get; private set; }

        private string script;
        public string Script
        {
            get { return script; }
            set
            {
                script = value;
                eventAggregator.Publish(new ScriptUpdatedEvent(value));
                NotifyOfPropertyChange(() => Script);
                provider.ScriptChanged(value, caretPosition);
                UpdateSignatureHelp();
            }
        }

        // The script can not be edited while it runs, but breakpoints can be changed and values inspected
        private bool isReadOnly;
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                isReadOnly = value;
                NotifyOfPropertyChange(() => IsReadOnly);
            }
        }

        public Caliburn.Micro.BindableCollection<Breakpoint> Breakpoints { get; private set; }

        private int executionLine;
        // The line where the script is paused, 0 when it's not paused
        public int ExecutionLine
        {
            get { return executionLine; }
            set
            {
                executionLine = value;
                NotifyOfPropertyChange(() => ExecutionLine);
            }
        }

        private int CaretLineNumber
        {
            get
            {
                var text = script ?? string.Empty;
                return text.Take(Math.Min(caretPosition, text.Length)).Count(c => c == '\n') + 1;
            }
        }

        private Breakpoint GetBreakpoint(int lineNumber)
        {
            return Breakpoints.FirstOrDefault(b => b.LineNumber == lineNumber);
        }

        public void ToggleBreakpoint()
        {
            var lineNumber = CaretLineNumber;
            var breakpoint = GetBreakpoint(lineNumber);

            if (breakpoint != null)
                Breakpoints.Remove(breakpoint);
            else
                Breakpoints.Add(new Breakpoint(lineNumber));
        }

        public IEnumerable<Caliburn.Micro.IResult> EditBreakpointCondition()
        {
            return EditBreakpointCondition(CaretLineNumber);
        }

        // Creates the breakpoint if the line has none, like Visual Studio
        public IEnumerable<Caliburn.Micro.IResult> EditBreakpointCondition(int lineNumber)
        {
            var existing = GetBreakpoint(lineNumber);
            var dialog = resultFactory.ShowDialog<BreakpointConditionViewModel>()
                .Configure(m => m.Init(lineNumber, existing != null ? existing.Condition : null, script));

            yield return dialog;

            if (!dialog.Model.IsAccepted)
                yield break;

            var breakpoint = new Breakpoint(lineNumber, dialog.Model.Condition);
            var index = Breakpoints.IndexOf(GetBreakpoint(lineNumber));

            if (index >= 0)
                Breakpoints[index] = breakpoint;
            else
                Breakpoints.Add(breakpoint);
        }

        public void DeleteAllBreakpoints()
        {
            Breakpoints.Clear();
        }

        private IScriptDebugger debugger;

        public void AttachDebugger(IScriptDebugger scriptDebugger)
        {
            debugger = scriptDebugger;
            debugger.SetBreakpoints(Breakpoints);
        }

        public void DetachDebugger()
        {
            debugger = null;
            ExecutionLine = 0;
        }

        private void OnBreakpointsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (debugger != null)
                debugger.SetBreakpoints(Breakpoints);
        }

        // Describes the value of the expression at index while the script is paused, null if there is nothing to show
        public string GetValueDescription(int index)
        {
            return debugger != null && debugger.IsPaused ? debugger.Evaluate(script, index) : null;
        }

        public Replacer Replacer { get; set; }

        private int caretPosition;
        public int CaretPosition
        {
            get { return caretPosition; }

            set
            {
                if (caretPosition == value) return;

                caretPosition = value;

                completion.CaretChanged();

                UpdateSignatureHelp();
                NotifyOfPropertyChange(() => CaretPosition);
            }
        }

        private void UpdateSignatureHelp()
        {
            if (SignatureHelp == null)
                return;

            var caret = Math.Min(caretPosition, (script ?? string.Empty).Length);
            SignatureHelp.Update(provider.GetSignatureHelp(script, caret));
        }

        private void OnInsertion(string insertedScript, int caretOffset, int range)
        {
            Replacer.Replace(caretOffset, range, insertedScript);
        }

        public override bool IsFileContent
        {
            get { return true; }
        }

        public override string Filename
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath))
                    return fileSystem.GetFilename(FilePath);

                var untitledPostfix = untitledId > 0 ? string.Format("-{0}", untitledId) : null;

                return string.Format("Untitled{0}.py", untitledPostfix);
            }
        }

        public override string Title
        {
            get
            {
                return Filename;
            }
        }

        public override string ToolTip => FilePath ?? Filename;

        public override string ContentId
        {
            get { return FilePath ?? Filename; }
        }


	    private int scriptHash;
		public bool IsDirty { get { return script == string.Empty || (!string.IsNullOrEmpty(script) && script.GetHashCode() != scriptHash); } }

        public override void Saved()
        {
			ResetDirtyFlag();
        }

        public void LoadFileContent(string content)
        {
            script = content;
            provider.ScriptChanged(content, 0);
			ResetDirtyFlag();
        }

	    private void ResetDirtyFlag()
	    {
			scriptHash = script.GetHashCode();
	    }

        public override string FileContent
        {
            get { return Script; }
        }

        public override string FilePath
        {
            get
            {
                return base.FilePath;
            }
            set
            {
                base.FilePath = value;
                NotifyOfPropertyChange(() => Title);
                NotifyOfPropertyChange(() => ToolTip);
                NotifyOfPropertyChange(() => ContentId);
            }
        }

        public void Handle(ScriptStateChangedEvent message)
        {
            IsReadOnly = message.Running;
        }
    }
}
