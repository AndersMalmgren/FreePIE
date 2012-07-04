using System.Linq;
using FreePIE.Core.ScriptEngine;
using FreePIE.GUI.Events;
using FreePIE.Core.Common.Events;

namespace FreePIE.GUI.Views.Script
{
    public class ScriptEditorViewModel : Caliburn.Micro.PropertyChangedBase, IHandle<ScriptStateChangedEvent>, IHandle<ScriptLoadedEvent>
    {
        private readonly IEventAggregator eventAggregator;

        public ScriptEditorViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            Enabled = true;
            eventAggregator.Subscribe(this);
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
