using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Caliburn.Micro;
using FreePIE.Core.Model.Events;
using FreePIE.GUI.CodeCompletion.Data;
using FreePIE.GUI.Events;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;
using Timer = System.Timers.Timer;

namespace FreePIE.GUI.Views.Script.Output
{
    public class WatchesViewModel : PropertyChangedBase, Core.Common.Events.IHandle<WatchEvent>, Core.Common.Events.IHandle<ScriptStateChangedEvent>
    {
        private readonly Timer updateTimer;

        public WatchesViewModel(IEventAggregator eventAggregator)
        {
            Watches = new BindableCollection<WatchViewModel>();
            eventAggregator.Subscribe(this);
            buffer = new Dictionary<string, WatchEvent>();

            updateTimer = new Timer(1);
            updateTimer.Elapsed += (x, y) => UpdateWatches();
            updateTimer.Start();
        }

        private Dictionary<string, WatchEvent> buffer;
        private readonly object locker = new object();

        private void UpdateWatches()
        {
            lock (locker)
            {
                var copy = new Dictionary<string, WatchEvent>(buffer);
                var toRemove = Watches.Where(w => !copy.Any(item => item.Key == w.Name)).ToList();
                Watches.RemoveRange(toRemove);
                AddOrUpdateWatches(copy);
            }
        }

        void AddOrUpdateWatches(Dictionary<string, WatchEvent> watchEvents)
        {
            var added = watchEvents.Where(ev => !Watches.Any(w => w.Name == ev.Key)).ToList();
            Watches.AddRange(added.Select(a => new WatchViewModel() { Name = a.Value.Name }));

            foreach (var watch in Watches)
                watch.Value = watchEvents[watch.Name].Value;
        }

        private void ClearBuffer()
        {
            buffer = new Dictionary<string, WatchEvent>();
        }

        public void Handle(WatchEvent message)
        {
            var tempBuffer = new Dictionary<string, WatchEvent>();
            tempBuffer[message.Name] = message;

            buffer = tempBuffer;
        }

        private void AddWatch(WatchEvent message)
        {
            var watch = Watches.FirstOrDefault(w => w.Name == message.Name);

            if (watch == null)
            {
                watch = new WatchViewModel { Name = message.Name };
                Watches.Add(watch);
            }
            watch.Value = message.Value;
        }

        public void Handle(ScriptStateChangedEvent message)
        {
            if (message.Running)
                ClearBuffer();
        }

        public BindableCollection<WatchViewModel> Watches { get; private set; }
    }
}