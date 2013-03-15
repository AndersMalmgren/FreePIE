using System;
using FreePIE.Core.Common;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.GUI.CodeCompletion;

namespace FreePIE.GUI.Common.CodeCompletion
{
    public class CompletionItem : ICompletionItem 
    {
        private readonly ExpressionInfo info;
        private readonly string token;
        private readonly Range replaceRange;
        private readonly string script;
        private readonly Action<string, int, int> insertionCallback;

        public CompletionItem(ExpressionInfo info, string token, Range replaceRange, string script, Action<string, int, int> insertionCallback)
        {
            this.info = info;
            this.token = token;
            this.replaceRange = replaceRange;
            this.script = script;
            this.insertionCallback = insertionCallback;
        }

        public void Insert()
        {
            insertionCallback(info.GetCompletion(token), replaceRange.Start, replaceRange.NumberOfElements);
        }

        public string Name
        {
            get { return info.GetFormattedName(); }
        }

        public string Description
        {
            get { return info.GetFormattedDescription(); }
        }
    }
}