using CompletionWindow;
using FreePIE.Core.ScriptEngine.CodeCompletion;

namespace FreePIE.GUI.Common.CodeCompletion
{
    public class CompletionItem : ICompletionItem
    {
        private readonly ExpressionInfo info;

        public CompletionItem(ExpressionInfo info)
        {
            this.info = info;
        }

        public void Insert()
        {
            string completion = info.GetCompletion();
        }

        public string Name { get { return info.Name; } }

        public string Description { get { return info.Description; }
        }
    }
}