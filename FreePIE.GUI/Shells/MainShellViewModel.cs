using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.Core.Persistence;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Views;
using FreePIE.GUI.Views.Main;
using FreePIE.GUI.Views.Plugin;
using FreePIE.GUI.Views.Script;
using FreePIE.GUI.Views.Script.Output;

namespace FreePIE.GUI.Shells
{
    public class MainShellViewModel : ShellPresentationModel, IHandle<RequestExitEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IPersistanceManager persistanceManager;

        public MainShellViewModel(IResultFactory resultFactory,
            IEventAggregator eventAggregator,
            IPersistanceManager persistanceManager,
            ISettingsManager settingsManager,
            MainMenuViewModel mainMenuViewModel,
            ScriptEditorViewModel scriptEditorViewModel,
            OutputViewModel outputViewModel)
            : base(resultFactory)
        {
            persistanceManager.Load();
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.persistanceManager = persistanceManager;

            Menu = mainMenuViewModel;
            Menu.Plugins = settingsManager.ListConfigurablePluginSettings().Select(ps => new PluginSettingsMenuViewModel(ps));
            ScriptEditor = scriptEditorViewModel;
            Output = outputViewModel;
            DisplayName = "FreePIE - Programmable Input Emulator";
        }

        public ScriptEditorViewModel ScriptEditor { get; set; }
        public OutputViewModel Output { get; set; }
        public MainMenuViewModel Menu { get; set; }

        public override void CanClose(Action<bool> callback)
        {
            eventAggregator.Publish(new ExitingEvent());

            persistanceManager.Save();
            base.CanClose(callback);
        }

        public void Handle(RequestExitEvent message)
        {
            TryClose();
        }
    }
}
