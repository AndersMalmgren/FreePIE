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
        private readonly AutoResetEvent detechEventSync = new AutoResetEvent(false);
        private readonly Queue<Action> eventQueue = new Queue<Action>();

        public void Subscribe(object subsriber)
        {
            subscribers.Add(subsriber);

            var detachEventWorker = new BackgroundWorker();
            detachEventWorker.DoWork += new DoWorkEventHandler(DetachEvents);
            detachEventWorker.RunWorkerAsync();
        }

        private void DetachEvents(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                detechEventSync.WaitOne();
                while(eventQueue.Count > 0)
                {
                    var action = eventQueue.Dequeue();
                    action();
                }
            }
        }

        public void Publish<T>(T message) where T : class
        {
            subscribers.OfType<IHandle<T>>()
            .ForEach(h => Handle(h, message));
        }

        private void Handle<T>(IHandle<T> handler, T message) where T : class
        {
            Action action = () => handler.Handle(message);
            if (handler is IHandleDetached<T>)
            {
                eventQueue.Enqueue(action);
                detechEventSync.Set();
            }
            else
                action();
        }
    }
}
