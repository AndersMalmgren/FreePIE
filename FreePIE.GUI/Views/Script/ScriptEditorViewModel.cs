using System.Linq;
using FreePIE.Core.Common;
using FreePIE.Core.ScriptEngine;
using FreePIE.GUI.CodeCompletion;
using FreePIE.GUI.CodeCompletion.Event.Actions;
using FreePIE.GUI.Common.AvalonEdit;
using FreePIE.GUI.Common.CodeCompletion;
using FreePIE.GUI.Events;
using FreePIE.Core.Common.Events;
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
            Enabled = true;
            UpdateCompletionItems();
            eventAggregator.Subscribe(this);
            completionModel.Observers.Add(new OpenOnWriteAction(IsBeginningOfExpression));
            completionModel.Observers.Add(new CloseOnSteppingIntoEndOfExpression(() => provider.IsBeginningOfExpression(Script, CaretPosition)));
            completionModel.Observers.Add(new CloseOnWritingEndOfExpression(IsEndOfExpression));
        }
        
        private static int untitledIndex = 0;
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
            var script = (Script ?? string.Empty).Insert(CaretPosition, nextChar.ToString());
            caret++;
            return provider.IsBeginningOfExpression(Script, CaretPosition) || provider.IsBeginningOfExpression(script, caret);
        }

        private bool IsEndOfExpression(char nextChar)
        {
            return provider.IsEndOfExpressionDelimiter(nextChar);
        }

        public CompletionPopupViewModel CompletionWindow { get; set; }

        private string script;
        public string Script
        {
            get { return script; }
            set
            {
                script = value;
                IsDirty = true;
                eventAggregator.Publish(new ScriptUpdatedEvent(value));
                NotifyOfPropertyChange(() => Script);
            }
        }

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set 
            { 
                enabled = value; 
                NotifyOfPropertyChange(() => Enabled);
            }
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
                UpdateCompletionItems();
                NotifyOfPropertyChange(() => CaretPosition);
            }
        }

        private void UpdateCompletionItems()
        {
            var codeResults = provider.GetSuggestionsForExpression(script, caretPosition);

            var suggestions = codeResults.ExpressionInfos.Select(x =>
                new CompletionItem(x, codeResults.ActiveToken.Value, codeResults.ReplaceRange, script, OnInsertion));

            CompletionWindow.CompletionItems.Clear();
            CompletionWindow.CompletionItems.AddRange(suggestions);
            if (CompletionWindow.CompletionItems.Any())
                CompletionWindow.SelectedCompletionItem = CompletionWindow.CompletionItems.First();
        }

        private void OnInsertion(string script, int caretOffset, int range)
        {
            Replacer.Replace(caretOffset, range, script);
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

        public override string ContentId
        {
            get { return FilePath ?? Filename; }
        }

        public bool IsDirty { get; set; }
        public override void Saved()
        {
            IsDirty = false;
        }

        public override string FileContent
        {
            get { return Script; }
            set
            {
                Script = value;
            }
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
                NotifyOfPropertyChange(() => ContentId);
            }
        }

        public void Handle(ScriptStateChangedEvent message)
        {
            Enabled = !message.Running;
        }
    }
}
