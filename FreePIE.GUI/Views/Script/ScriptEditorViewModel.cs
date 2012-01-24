using Caliburn.Micro;
using FreePIE.GUI.Events;

namespace FreePIE.GUI.Views.Script
{
    public class ScriptEditorViewModel : PropertyChangedBase
    {
        private readonly IEventAggregator eventAggregator;

        public ScriptEditorViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

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
    }
}
