using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.GUI.CodeCompletion;
using FreePIE.GUI.CodeCompletion.Event.Actions;

namespace FreePIE.GUI.Common.CodeCompletion
{
    // Keeps the completion popup filled with the suggestions for the script at the caret
    public class ScriptCompletion
    {
        private readonly ICodeCompletionProvider provider;
        private readonly CompletionPopupViewModel popup;
        private readonly Func<string> getScript;
        private readonly Func<int> getCaret;
        private readonly Action<string, int, int> insert;

        private bool isOpen;
        private string completionScript;
        private int completionCaret = -1;
        private IList<ExpressionInfo> completionInfos;

        public ScriptCompletion(ICodeCompletionProvider provider, CompletionPopupViewModel popup, Func<string> getScript, Func<int> getCaret, Action<string, int, int> insert)
        {
            this.provider = provider;
            this.popup = popup;
            this.getScript = getScript;
            this.getCaret = getCaret;
            this.insert = insert;

            popup.Observers.Add(new PopupStateAction(OnPopupStateChanged));
            popup.Observers.Add(new OpenOnWriteAction(IsBeginningOfExpression));
            popup.Observers.Add(new CloseOnSteppingIntoEndOfExpression(() => provider.IsBeginningOfExpression(getScript(), getCaret())));
            popup.Observers.Add(new CloseOnWritingEndOfExpression(provider.IsEndOfExpressionDelimiter));
        }

        // Suggestions are only calculated while they are shown, it's too expensive to do on every keystroke
        public void CaretChanged()
        {
            if (isOpen)
                Update();
        }

        // The script has been analyzed in the background, suggestions may include new names
        public void ScriptAnalyzed()
        {
            if (isOpen)
                Update(true);
        }

        private bool IsBeginningOfExpression(char nextChar)
        {
            var script = getScript();
            var caret = getCaret();
            var scriptWithNextChar = (script ?? string.Empty).Insert(caret, nextChar.ToString(CultureInfo.InvariantCulture));
            var isBeginning = provider.IsBeginningOfExpression(script, caret) || provider.IsBeginningOfExpression(scriptWithNextChar, caret + 1);

            // The popup is only shown if there are suggestions, so they are needed before it opens
            if (isBeginning)
                Update();

            return isBeginning;
        }

        private void OnPopupStateChanged(bool open)
        {
            isOpen = open;

            if (open)
                Update();
        }

        private void Update(bool force = false)
        {
            var script = getScript();
            var caret = getCaret();

            var isSamePosition = completionScript == script && completionCaret == caret;
            if (!force && isSamePosition)
                return;

            completionScript = script;
            completionCaret = caret;

            var codeResults = provider.GetSuggestionsForExpression(script, caret);

            // Rebuilding the list is expensive in WPF and the background analysis often finds the same suggestions again
            var infos = codeResults.ExpressionInfos.ToList();
            var isSame = isSamePosition && IsSameSuggestions(completionInfos, infos);
            completionInfos = infos;
            if (isSame)
                return;

            var suggestions = infos.Select(x =>
                new CompletionItem(x, codeResults.ActiveToken.Value, codeResults.ReplaceRange, script, insert));

            popup.CompletionItems.Clear();
            popup.CompletionItems.AddRange(suggestions);
            if (popup.CompletionItems.Any())
                popup.SelectedCompletionItem = GetBestMatch(popup.CompletionItems, codeResults.ActiveToken.Value);
        }

        private static bool IsSameSuggestions(IList<ExpressionInfo> previous, IList<ExpressionInfo> current)
        {
            return previous != null && previous.Count == current.Count && previous.Zip(current, (a, b) => a.Name == b.Name && a.Kind == b.Kind && a.Description == b.Description).All(same => same);
        }

        // Prefer the suggestion with the same casing as what has been written (axis before Axis)
        private static ICompletionItem GetBestMatch(IList<ICompletionItem> items, string token)
        {
            return items.FirstOrDefault(i => i.Name == token)
                   ?? items.FirstOrDefault(i => i.Name.StartsWith(token, StringComparison.Ordinal))
                   ?? items.First();
        }
    }
}
