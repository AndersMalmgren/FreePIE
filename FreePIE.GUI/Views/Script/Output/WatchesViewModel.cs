using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Caliburn.Micro;
using FreePIE.Core.Model.Events;
using FreePIE.GUI.Events;
using FreePIE.GUI.Views.Main;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Script.Output
{
    public class WatchesViewModel : PanelViewModel, Core.Common.Events.IHandle<WatchEvent>, Core.Common.Events.IHandle<ScriptStateChangedEvent>
    {
        public WatchesViewModel(IEventAggregator eventAggregator)
        {
            Watches = new BindableCollection<WatchViewModel>();
            eventAggregator.Subscribe(this);
            messages = new Dictionary<string, object>();

            ThreadPool.QueueUserWorkItem(x =>
                {
                    while (true)
                    {
                        if(clearWatches)
                        {
                            clearWatches = false;
                            Watches.Clear();
                        }
                        foreach (var message in messages)
                            AddWatch(message.Key, message.Value);
                        Thread.Sleep(20);
                    }
                });

            Title = "Watch";
            IconName = "watch-16.png";
        }

        private volatile bool clearWatches;
        private volatile Dictionary<string, object> messages;

        public void Handle(WatchEvent message)
        {
            var tempMessage = new Dictionary<string, object>(messages);
            tempMessage[message.Name] = message.Value;
            messages = tempMessage;
        }

        private void AddWatch(string name, object message)
        {
            var watch = Watches.FirstOrDefault(w => w.Name == name);

            if (watch == null)
            {
                watch = new WatchViewModel { Name = name };
                Watches.Add(watch);
            }

            watch.Value = message;
        }

        public void Handle(ScriptStateChangedEvent message)
        {
            if (message.Running)
            {
                messages = new Dictionary<string, object>();
                clearWatches = true;
            }
        }

        public BindableCollection<WatchViewModel> Watches { get; private set; }
    }
}
