using System;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model.Events;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [Global(Name = "diagnostics")]
    public class DiagnosticHelper : IScriptHelper
    {
        private readonly IEventAggregator eventAggregator;

        public DiagnosticHelper(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void debug(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void debug(object arg)
        {
            Console.WriteLine(arg);
        }

        [NeedIndexer]
        public void watch(object value, string indexer)
        {
            eventAggregator.Publish(new WatchEvent(indexer, value));
        }

        public void notify(string message)
        {
            eventAggregator.Publish(new TrayNotificationEvent(message, ""));

        }
        public void notify(string title, string message)
        {
            eventAggregator.Publish(new TrayNotificationEvent(message, title));

        }
        public void notify(string title,string message, params object[] args)
        {
            eventAggregator.Publish(new TrayNotificationEvent(string.Format(message, args),title));
           
        }

        
    }
}
