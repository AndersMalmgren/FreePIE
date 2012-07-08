using Caliburn.Micro;

namespace FreePIE.GUI.CodeCompletion
{

    public class CompletionPopupViewModel : PropertyChangedBase
    {
        private BindableCollection<ICompletionItem> completionItems;

        public CompletionPopupViewModel()
        {
            this.completionItems = new BindableCollection<ICompletionItem>();
        }

        public BindableCollection<ICompletionItem> CompletionItems
        {
            get { return completionItems; }
            set
            {
                completionItems = value;
                NotifyOfPropertyChange(() => CompletionItems);
            }
        }
    }
}
