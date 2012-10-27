using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using FreePIE.Core.Common.Extensions;

namespace FreePIE.Core.Common.Events
{
    public class EventAggregator : IEventAggregator
    {
        private readonly WeakReferenceList<object> subscribers = new WeakReferenceList<object>();
        private readonly AutoResetEvent detachEventSync = new AutoResetEvent(false);
        private volatile Dictionary<int, Action> eventQueue = new Dictionary<int, Action>();

        public EventAggregator ()
        {
            var detachEventWorker = new BackgroundWorker();
            detachEventWorker.DoWork += DetachEvents;
            detachEventWorker.RunWorkerAsync();
        }

        public void Subscribe(object subsriber)
        {
            subscribers.Add(subsriber);
        }

        private void DetachEvents(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                detachEventSync.WaitOne();
                var tempQueue = Interlocked.Exchange(ref eventQueue, new Dictionary<int, Action>());
                tempQueue.ForEach(pair => pair.Value());
            }
        }

        public void Publish<T>(T message) where T : class
        {
            subscribers.OfType<IHandle<T>>()
            .ForEach(h => Handle(h, message));

            detachEventSync.Set();
        }

        private void Handle<T>(IHandle<T> handler, T message) where T : class
        {
            Action action = () => handler.Handle(message);
            if (handler is IHandleDetached<T>)
            {
                var tempQueue = new Dictionary<int, Action>(eventQueue);
                tempQueue[message.GetHashCode()] = action;
                eventQueue = tempQueue;
            }
            else
                action();
        }
    }
}
