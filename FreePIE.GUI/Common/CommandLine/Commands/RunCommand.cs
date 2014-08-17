using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Common.Events;
using FreePIE.GUI.Events.Command;

namespace FreePIE.GUI.Common.CommandLine.Commands
{
    public class RunCommand : Command<RunEvent>
    {
        public RunCommand(IEventAggregator eventAggregator) : base(eventAggregator) { }

        public override IEnumerable<string> Keys
        {
            get { return new[] {"r", "run"}; }
        }
    }
}
