using System;
using System.Linq;
using Caliburn.Micro;
using FreePIE.Core.Common.Extensions;
using FreePIE.Core.ScriptEngine;
using FreePIE.GUI.CodeCompletion;
using FreePIE.GUI.CodeCompletion.Event.Actions;
using FreePIE.GUI.Common;
using FreePIE.GUI.Common.AvalonEdit;
using FreePIE.GUI.Common.CodeCompletion;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Script
{
    public class ScriptEditorViewModel : PropertyChangedBase, IHandle<ScriptStateChangedEvent>, IHandle<ScriptLoadedEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ICodeCompletionProvider provider;

        private int ClampToZero(int i)
        {
            return i < 0 ? 0 : i;
        }

        public ScriptEditorViewModel(IEventAggregator eventAggregator, ICodeCompletionProvider provider, CompletionPopupViewModel completionModel)
        {
            this.eventAggregator = eventAggregator;
            this.provider = provider;
            CompletionWindow = completionModel;
            Enabled = true;
            eventAggregator.Subscribe(this);
            completionModel.Observers.Add(new OpenOnWriteAction(() => provider.IsBeginningOfExpression(Script, CaretPosition)));
            completionModel.Observers.Add(new CloseOnSteppingIntoEndOfExpression(() => provider.IsBeginningOfExpression(Script, ClampToZero(CaretPosition - 1))));
        }

        public CompletionPopupViewModel CompletionWindow { get; set; }

        private string script;
        public string Script
        {
            get { return script; }
            set
            {
                script = value; 
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

        private void OnInsertion(string script, int caretOffset)
        {
            Script = script;
            CaretPosition = caretOffset;
        }

        public void Handle(ScriptStateChangedEvent message)
        {
            Enabled = !message.Running;
        }

        public void Handle(ScriptLoadedEvent message)
        {
            Script = message.Script;
        }
    }
}
