using System.Collections.Generic;
using FreePIE.Core.Common.Events;
using FreePIE.GUI.Events.Command;

namespace FreePIE.GUI.Common.CommandLine.Commands
{
    public abstract class Command
    {
        public virtual IEnumerable<string> Keys { get { return new List<string>(); } }
        public abstract void Execute(string data);
    }

    public abstract class Command<TEVent> : Command where TEVent : CommandEvent, new()
    {
        private readonly IEventAggregator eventAggregator;

        protected Command(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }


        public override void Execute(string data)
        {
            var message = new TEVent();
            message.Data = data;
            eventAggregator.Publish(message);
        }
    }
}
