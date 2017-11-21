using System.Collections.Generic;
using FreePIE.Core.Common.Events;
using FreePIE.GUI.Events.Command;

namespace FreePIE.GUI.Common.CommandLine.Commands
{
    public class TrayCommand : Command<TrayEvent>
    {
        public TrayCommand(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public override IEnumerable<string> Keys
        {
            get { return new[] { "t", "tray" }; }
        }
    }
}