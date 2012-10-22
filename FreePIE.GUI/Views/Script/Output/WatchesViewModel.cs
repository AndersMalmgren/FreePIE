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
        private DateTime lastUpdate;
        private static TimeSpan interval = TimeSpan.FromMilliseconds(20);

        public WatchesViewModel(IEventAggregator eventAggregator)
        {
            Watches = new BindableCollection<WatchViewModel>();
            eventAggregator.Subscribe(this);
            lastUpdate = DateTime.Now;
        }

        public void Handle(WatchEvent message)
        {
            if (DateTime.Now - lastUpdate > interval)
            {
                lastUpdate = DateTime.Now;
                AddWatch(message, false);
            }
        }

        private WatchViewModel AddWatch(WatchEvent message, bool locked)
        {
            var watch = Watches.FirstOrDefault(w => w.Name == message.Name);
            
            if (watch == null)
            {
                if (locked)
                {
                    watch = new WatchViewModel();
                    watch.Name = message.Name;
                    Watches.Add(watch);
                }
                else
                {
                    lock (Watches)
                    {
                        watch = AddWatch(message, true);
                    }
                }
            }
            watch.Value = message.Value;

            return watch;
        }

        public void Handle(ScriptStateChangedEvent message)
        {
            if(message.Running)
                Watches.Clear();
        }

        public BindableCollection<WatchViewModel> Watches { get; private set; }

    }
}
