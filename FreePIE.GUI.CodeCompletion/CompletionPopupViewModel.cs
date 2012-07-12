using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using FreePIE.GUI.CodeCompletion.Event;

namespace FreePIE.GUI.CodeCompletion
{

    public class CompletionPopupViewModel : PropertyChangedBase
    {
        private BindableCollection<ICompletionItem> completionItems;
        private ICompletionItem completionItem;


        public CompletionPopupViewModel()
        {
            this.completionItems = new BindableCollection<ICompletionItem>();
            Observers = new List<IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>>();
        }

        public BindableCollection<ICompletionItem> CompletionItems
        {
            get { return completionItems; }
            set
            {
                completionItems = value;
                NotifyOfPropertyChange(() => CompletionItems);
                SelectedCompletionItem = completionItems.FirstOrDefault();
            }
        }

        public ICompletionItem SelectedCompletionItem
        {
            get { return completionItem; }
            set
            {
                completionItem = value;
                NotifyOfPropertyChange(() => SelectedCompletionItem);
            }
        }

        public IList<IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>> Observers 
        { get; private set; }

        public void SelectNextCompletionItem()
        {
            int index = CompletionItems.IndexOf(SelectedCompletionItem);

            if (index != CompletionItems.Count - 1)
                SelectedCompletionItem = CompletionItems[index + 1];
        }

        public void SelectPreviousCompletionItem()
        {
            int index = CompletionItems.IndexOf(SelectedCompletionItem);

            if (index != 0)
                SelectedCompletionItem = CompletionItems[index - 1];
        }
    }
}
