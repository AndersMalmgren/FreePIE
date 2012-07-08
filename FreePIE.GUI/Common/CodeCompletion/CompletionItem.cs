using System;
using FreePIE.Core.Common;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.ScriptEngine.CodeCompletion;
using FreePIE.GUI.CodeCompletion;

namespace FreePIE.GUI.Common.CodeCompletion
{
    public class CompletionItem : ICompletionItem 
    {
        private readonly ExpressionInfo info;
        private readonly Range replaceRange;
        private readonly string script;
        private readonly Action<string, int> insertionCallback;

        public CompletionItem(ExpressionInfo info, Range replaceRange, string script, Action<string, int> insertionCallback)
        {
            this.info = info;
            this.replaceRange = replaceRange;
            this.script = script;
            this.insertionCallback = insertionCallback;
        }

        public void Insert()
        {
            int newOffset = replaceRange.Start + info.GetCompletion().Length;
            insertionCallback(script.Replace(replaceRange, info.GetCompletion()), newOffset);
        }

        public string Name { get { return info.Name; } }

        public string Description { get { return info.Description; }
        }
    }
}