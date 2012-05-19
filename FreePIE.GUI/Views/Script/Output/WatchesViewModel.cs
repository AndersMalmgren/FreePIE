using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.Core.Model.Events;
using FreePIE.GUI.Events;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Script.Output
{
    public class WatchesViewModel : PropertyChangedBase, Core.Common.Events.IHandle<WatchEvent>, Core.Common.Events.IHandle<ScriptStateChangedEvent>
    {
        public WatchesViewModel(IEventAggregator eventAggregator)
        {
            Watches = new BindableCollection<WatchViewModel>();
            eventAggregator.Subscribe(this);
        }

        public void Handle(WatchEvent message)
        {
            var watch = Watches.FirstOrDefault(w => w.Name == message.Name);
            if(watch == null)
            {
                watch = new WatchViewModel();
                watch.Name = message.Name;
                Watches.Add(watch);
            }

            watch.Value = message.Value;
        }

        public void Handle(ScriptStateChangedEvent message)
        {
            if(message.Running)
                Watches.Clear();
        }

        public BindableCollection<WatchViewModel> Watches { get; private set; }

    }
}
