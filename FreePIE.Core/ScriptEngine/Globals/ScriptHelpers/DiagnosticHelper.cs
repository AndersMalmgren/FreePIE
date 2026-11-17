using System;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model.Events;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [Global(Name = "diagnostics")]
    [Documentation("Debug output, watches and tray notifications")]
    public class DiagnosticHelper : IScriptHelper
    {
        private readonly IEventAggregator eventAggregator;

        public DiagnosticHelper(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        [Documentation("Writes a formatted line to the console, format uses .NET composite formatting ({0}, {1} ...)")]
        public void debug(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        [Documentation("Writes a value to the console")]
        public void debug(object arg)
        {
            Console.WriteLine(arg);
        }

        [NeedIndexer]
        [Documentation("Shows the value in the watch window, named after the expression")]
        public void watch(object value, string indexer)
        {
            eventAggregator.Publish(new WatchEvent(indexer, value));
        }

        [Documentation("Shows a tray notification")]
        public void notify(string message)
        {
            eventAggregator.Publish(new TrayNotificationEvent(message, ""));

        }
        [Documentation("Shows a tray notification with a title")]
        public void notify(string title, string message)
        {
            eventAggregator.Publish(new TrayNotificationEvent(message, title));

        }
        [Documentation("Shows a tray notification with a title, message uses .NET composite formatting ({0}, {1} ...)")]
        public void notify(string title,string message, params object[] args)
        {
            eventAggregator.Publish(new TrayNotificationEvent(string.Format(message, args),title));
           
        }

        
    }
}
