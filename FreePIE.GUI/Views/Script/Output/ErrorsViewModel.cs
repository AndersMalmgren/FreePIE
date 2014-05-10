using System.Collections.Generic;
using System.Windows.Media;
using Caliburn.Micro;
using FreePIE.Core.Model.Events;
using FreePIE.GUI.Common.Resources;
using FreePIE.GUI.Events;
using FreePIE.GUI.Views.Main;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Script.Output
{
    public class ErrorsViewModel : PanelViewModel, Core.Common.Events.IHandle<ScriptErrorEvent>, Core.Common.Events.IHandle<ScriptStateChangedEvent>
    {
        private static readonly Dictionary<ErrorLevel, ImageSource> levelImages = new Dictionary<ErrorLevel, ImageSource>
            {
                { ErrorLevel.Warning,  ResourceHelper.Load("warning-16.png") },
                { ErrorLevel.Exception, ResourceHelper.Load("exception-16.png") }
            };

        public BindableCollection<ErrorViewModel> Errors { get; private set; }

        public ErrorsViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);

            Errors = new BindableCollection<ErrorViewModel>();
            Title = "Error";
            IconName = "error-16.png";
        }

        public void Handle(ScriptErrorEvent message)
        {
            Errors.Add(new ErrorViewModel(message.Description, levelImages[message.Level], message.LineNumber));
            IsActive = true;
        }

        public void Handle(ScriptStateChangedEvent message)
        {
            if (message.Running)
                Errors.Clear();
        }
    }
}
