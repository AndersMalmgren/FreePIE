using System.Collections.Generic;
using Caliburn.Micro;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.GUI.CodeCompletion;
using FreePIE.GUI.Common.AvalonEdit;
using FreePIE.GUI.Common.CodeCompletion;
using FreePIE.GUI.Result;

namespace FreePIE.GUI.Shells
{
    public class BreakpointConditionViewModel : ShellPresentationModel
    {
        private readonly ICodeCompletionProvider provider;
        private readonly ScriptCompletion completion;
        private BreakpointConditionScript conditionScript = new BreakpointConditionScript(string.Empty, 1);

        public BreakpointConditionViewModel(IResultFactory resultFactory, ICodeCompletionProvider provider, CompletionPopupViewModel completionWindow) : base(resultFactory)
        {
            this.provider = provider;
            CompletionWindow = completionWindow;
            Replacer = new Replacer();
            completion = new ScriptCompletion(provider, completionWindow, () => ContextScript, () => conditionScript.ToScriptOffset(caretPosition), OnInsertion);
            provider.ScriptAnalyzed += (sender, args) => Execute.BeginOnUIThread(completion.ScriptAnalyzed);
        }

        public void Init(int lineNumber, string condition, string script)
        {
            DisplayName = string.Format("Breakpoint condition - line {0}", lineNumber);
            conditionScript = new BreakpointConditionScript(script, lineNumber);
            Condition = condition;
        }

        public CompletionPopupViewModel CompletionWindow { get; private set; }

        public Replacer Replacer { get; private set; }

        // The script with the condition at the breakpoint line, so that the names visible there are suggested
        private string ContextScript
        {
            get { return conditionScript.GetScript(condition); }
        }

        private string condition;
        public string Condition
        {
            get { return condition; }
            set
            {
                condition = value;
                NotifyOfPropertyChange(() => Condition);
                provider.ScriptChanged(ContextScript, conditionScript.ToScriptOffset(caretPosition));
            }
        }

        private int caretPosition;
        public int CaretPosition
        {
            get { return caretPosition; }
            set
            {
                if (caretPosition == value)
                    return;

                caretPosition = value;
                completion.CaretChanged();
                NotifyOfPropertyChange(() => CaretPosition);
            }
        }

        private void OnInsertion(string insertedText, int offset, int length)
        {
            Replacer.Replace(conditionScript.ToConditionOffset(offset), length, insertedText);
        }

        public bool IsAccepted { get; private set; }

        public IEnumerable<IResult> Ok()
        {
            IsAccepted = true;
            yield return Result.Close();
        }
    }
}
