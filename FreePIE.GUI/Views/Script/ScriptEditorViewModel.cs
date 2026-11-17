using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Events;
using FreePIE.Core.ScriptEngine;
using FreePIE.GUI.CodeCompletion;
using FreePIE.GUI.CodeCompletion.Event.Actions;
using FreePIE.GUI.Common.AvalonEdit;
using FreePIE.GUI.Common.CodeCompletion;
using FreePIE.GUI.Events;
using FreePIE.GUI.Views.Main;

namespace FreePIE.GUI.Views.Script
{
    public class ScriptEditorViewModel : PanelViewModel, IHandle<ScriptStateChangedEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ICodeCompletionProvider provider;
        private readonly FileSystem fileSystem;

        public ScriptEditorViewModel(IEventAggregator eventAggregator, ICodeCompletionProvider provider, CompletionPopupViewModel completionModel, FileSystem fileSystem)
        {
            this.eventAggregator = eventAggregator;
            this.provider = provider;
            this.fileSystem = fileSystem;
            Replacer = new Replacer();
            CompletionWindow = completionModel;
            SignatureHelp = new SignatureHelpViewModel();
            Breakpoints = new Caliburn.Micro.BindableCollection<int>();
            Breakpoints.CollectionChanged += OnBreakpointsChanged;
            eventAggregator.Subscribe(this);
            provider.ScriptAnalyzed += (sender, args) => Caliburn.Micro.Execute.BeginOnUIThread(OnScriptAnalyzed);
            completionModel.Observers.Add(new PopupStateAction(OnCompletionPopupStateChanged));
            completionModel.Observers.Add(new OpenOnWriteAction(IsBeginningOfExpression));
            completionModel.Observers.Add(new CloseOnSteppingIntoEndOfExpression(() => provider.IsBeginningOfExpression(Script, CaretPosition)));
            completionModel.Observers.Add(new CloseOnWritingEndOfExpression(IsEndOfExpression));
        }
        
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

        private bool IsBeginningOfExpression(char nextChar)
        {
            var caret = CaretPosition;
            var scriptWithNextChar = (Script ?? string.Empty).Insert(CaretPosition, nextChar.ToString(CultureInfo.InvariantCulture));
            caret++;
            var isBeginning = provider.IsBeginningOfExpression(Script, CaretPosition) || provider.IsBeginningOfExpression(scriptWithNextChar, caret);

            // The popup is only shown if there are suggestions, so they are needed before it opens
            if (isBeginning)
                UpdateCompletionItems();

            return isBeginning;
        }

        private void OnCompletionPopupStateChanged(bool isOpen)
        {
            isCompletionOpen = isOpen;

            if (isOpen)
                UpdateCompletionItems();
        }

        // The script has been analyzed in the background, suggestions may include new names
        private void OnScriptAnalyzed()
        {
            if (isCompletionOpen)
                UpdateCompletionItems(true);

            UpdateSignatureHelp();
        }

        private bool IsEndOfExpression(char nextChar)
        {
            return provider.IsEndOfExpressionDelimiter(nextChar);
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

        public Caliburn.Micro.BindableCollection<int> Breakpoints { get; private set; }

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

        public void ToggleBreakpoint()
        {
            var text = script ?? string.Empty;
            var lineNumber = text.Take(Math.Min(caretPosition, text.Length)).Count(c => c == '\n') + 1;

            if (!Breakpoints.Remove(lineNumber))
                Breakpoints.Add(lineNumber);
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

                // Suggestions are only calculated while they are shown, it's too expensive to do on every keystroke
                if (isCompletionOpen)
                    UpdateCompletionItems();

                UpdateSignatureHelp();
                NotifyOfPropertyChange(() => CaretPosition);
            }
        }

        private bool isCompletionOpen;
        private string completionScript;
        private int completionCaret = -1;

        private void UpdateCompletionItems(bool force = false)
        {
            if (!force && ReferenceEquals(completionScript, script) && completionCaret == caretPosition)
                return;

            completionScript = script;
            completionCaret = caretPosition;

            var codeResults = provider.GetSuggestionsForExpression(script, caretPosition);

            var suggestions = codeResults.ExpressionInfos.Select(x =>
                new CompletionItem(x, codeResults.ActiveToken.Value, codeResults.ReplaceRange, script, OnInsertion));

            CompletionWindow.CompletionItems.Clear();
            CompletionWindow.CompletionItems.AddRange(suggestions);
            if (CompletionWindow.CompletionItems.Any())
                CompletionWindow.SelectedCompletionItem = GetBestMatch(CompletionWindow.CompletionItems, codeResults.ActiveToken.Value);
        }

        // Prefer the suggestion with the same casing as what has been written (axis before Axis)
        private static ICompletionItem GetBestMatch(IList<ICompletionItem> items, string token)
        {
            return items.FirstOrDefault(i => i.Name == token)
                   ?? items.FirstOrDefault(i => i.Name.StartsWith(token, StringComparison.Ordinal))
                   ?? items.First();
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
