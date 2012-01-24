using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.GUI.Events;

namespace FreePIE.GUI.Views.Script.Output
{
    public class ErrorViewModel : PropertyChangedBase, IHandle<ScriptErrorEvent>
    {
        private readonly IEventAggregator eventAggregator;

        public ErrorViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                NotifyOfPropertyChange(() => Text);
            }
        }

        private bool error;
        public bool Error
        {
            get { return error; }
            set 
            {
                error = value;
                NotifyOfPropertyChange(() => Error);
            }
        }

        public void Handle(ScriptErrorEvent message)
        {
            Error = true;
            Text += string.Format("{0}{1}", message.Exception.Message, Environment.NewLine);
        }
    }
}
