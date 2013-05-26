using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion
{
    public class ItemClickedEventArgs
    {
        public MouseEventArgs Args { get; private set; }
        public ICompletionItem CompletionItem { get; private set; }

        public ItemClickedEventArgs(MouseEventArgs args, ICompletionItem completionItem)
        {
            Args = args;
            CompletionItem = completionItem;
        }
    }
}