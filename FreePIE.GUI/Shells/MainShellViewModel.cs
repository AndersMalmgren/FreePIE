using System;
using System.Linq;
using FreePIE.Core.Persistence;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Views.Main;
using FreePIE.GUI.Views.Plugin;
using FreePIE.GUI.Views.Script;
using FreePIE.GUI.Views.Script.Output;
using FreePIE.Core.Common.Events;

namespace FreePIE.GUI.Shells
{
    public class MainShellViewModel : ShellPresentationModel
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IPersistanceManager persistanceManager;

        public MainShellViewModel(IResultFactory resultFactory,
            IEventAggregator eventAggregator,
            IPersistanceManager persistanceManager,
            ISettingsManager settingsManager,
            MainMenuViewModel mainMenuViewModel,
            ScriptEditorViewModel scriptEditorViewModel,
            OutputViewModel outputViewModel) : base(resultFactory)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.persistanceManager = persistanceManager;

            Menu = mainMenuViewModel;
            Menu.Plugins = settingsManager.ListConfigurablePluginSettings().Select(ps => new PluginSettingsMenuViewModel(ps));
            Menu.HelpFiles = settingsManager.ListPluginSettingsWithHelpFile().Select(ps => new PluginHelpFileViewModel(ps)).ToList();

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
    }
}
