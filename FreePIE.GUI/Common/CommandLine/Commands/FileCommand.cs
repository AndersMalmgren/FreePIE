using FreePIE.Core.Common.Events;
using FreePIE.GUI.Events.Command;

namespace FreePIE.GUI.Common.CommandLine.Commands
{
    public class FileCommand : Command<FileEvent>
    {
        public FileCommand(IEventAggregator eventAggregator) : base(eventAggregator) {}
    }
}
