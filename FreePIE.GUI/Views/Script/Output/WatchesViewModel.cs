using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Caliburn.Micro;
using FreePIE.Core.Model.Events;
using FreePIE.GUI.CodeCompletion.Data;
using FreePIE.GUI.Events;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Script.Output
{
    public class WatchesViewModel : PropertyChangedBase, Core.Common.Events.IHandle<WatchEvent>, Core.Common.Events.IHandle<ScriptStateChangedEvent>
    {
        private Timer updateTimer;

        public WatchesViewModel(IEventAggregator eventAggregator)
        {
            Watches = new BindableCollection<WatchViewModel>();
            buffer = new Dictionary<WatchViewModel, object>();
            eventAggregator.Subscribe(this);
            buffer = new Dictionary<WatchViewModel, object>();
            updateTimer = new Timer(20);
            updateTimer.Elapsed += (x, y) => UpdateWatches();
            updateTimer.Start();
        }

        private Dictionary<WatchViewModel, object> buffer;

        private void UpdateWatches()
        {
            lock (buffer)
                foreach (var pair in buffer)
                    pair.Key.Value = pair.Value;
        }

        private void SetWatchValue(WatchViewModel watch, object value)
        {
            lock (buffer)
                buffer[watch] = value;
        }

        private void ClearWatchBuffer()
        {
            lock(buffer)
                buffer.Clear();
        }

        public void Handle(WatchEvent message)
        {
            AddWatch(message, false);
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
            SetWatchValue(watch, message.Value);

            return watch;
        }

        public void Handle(ScriptStateChangedEvent message)
        {
            if (message.Running)
            {
                ClearWatchBuffer();
                Watches.Clear();
            }
        }

        public BindableCollection<WatchViewModel> Watches { get; private set; }

    }
}
