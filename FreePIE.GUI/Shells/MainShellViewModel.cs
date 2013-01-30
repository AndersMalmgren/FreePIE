using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AvalonDock;
using AvalonDock.Layout.Serialization;
//using Caliburn.Micro;
using Caliburn.Micro;
using FreePIE.Core.Common;
using FreePIE.Core.Persistence;
using FreePIE.GUI.Common.AvalonDock;
using FreePIE.GUI.Common.Strategies;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Views.Main;
using FreePIE.GUI.Views.Plugin;
using FreePIE.GUI.Views.Script;
using FreePIE.GUI.Views.Script.Output;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Shells
{
    public class MainShellViewModel : ShellPresentationModel, Core.Common.Events.IHandle<ScriptDocumentAddedEvent>
    {
        private const string dockingConfig = "layout.config";
        private readonly IEventAggregator eventAggregator;
        private readonly IPersistanceManager persistanceManager;
        private readonly IFileSystem fileSystem;
        private readonly ScriptDialogStrategy scriptDialogStrategy;

        public MainShellViewModel(IResultFactory resultFactory,
                                  IEventAggregator eventAggregator,
                                  IPersistanceManager persistanceManager,
                                  ISettingsManager settingsManager,
                                  MainMenuViewModel mainMenuViewModel,
                                  ConsoleViewModel consoleViewModel,
                                  ErrorViewModel errorViewModel,
                                  WatchesViewModel watchesViewModel,
                                  IFileSystem fileSystem,
                                  ScriptDialogStrategy scriptDialogStrategy
                                  )
            : base(resultFactory)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.persistanceManager = persistanceManager;
            this.fileSystem = fileSystem;
            this.scriptDialogStrategy = scriptDialogStrategy;

            Scripts = new BindableCollection<ScriptEditorViewModel>();
            Tools = new BindableCollection<PanelViewModel>();

            Tools.Add(consoleViewModel);
            Tools.Add(errorViewModel);
            Tools.Add(watchesViewModel);

            Menu = mainMenuViewModel;
            Menu.Plugins = settingsManager.ListConfigurablePluginSettings().Select(ps => new PluginSettingsMenuViewModel(ps));
            Menu.HelpFiles = settingsManager.ListPluginSettingsWithHelpFile().Select(ps => new PluginHelpFileViewModel(ps)).ToList();
            Menu.Views = Tools;

            DisplayName = "FreePIE - Programmable Input Emulator";
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            InitDocking();
        }

        private void InitDocking()
        {
            if (!fileSystem.Exists(dockingConfig)) return;

            var layoutSerializer = new XmlLayoutSerializer(DockingManager);
            layoutSerializer.Deserialize(dockingConfig);
        }

        private DockingManager DockingManager
        {
            get { return (this.GetView() as IDockingManagerSource).DockingManager; }
        }

        private PanelViewModel activeDocument;
        public PanelViewModel ActiveDocument
        {
            get { return activeDocument; }
            set
            {
                    
                if (value == null || value.IsFileContent)
                {
                    activeDocument = value;
                    NotifyOfPropertyChange(() => ActiveDocument);
                    eventAggregator.Publish(new ActiveScriptDocumentChangedEvent(value));
                }
            }
        }

        public IEnumerable<IResult> DocumentClosing(ScriptEditorViewModel document, DocumentClosingEventArgs e)
        {
            if (document.IsDirty)
            {
                var message = Result.ShowMessageBox(document.Filename, string.Format("Do you want to save changes to {0}", document.Filename), MessageBoxButton.YesNoCancel);
                yield return message;
                
                if (message.Result == System.Windows.MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if (message.Result == System.Windows.MessageBoxResult.Yes)
                {
                    foreach (var result in scriptDialogStrategy.SaveAs(document, true, path => fileSystem.WriteAllText(path, document.FileContent)))
                        yield return result;
                }
            }
        }

        public BindableCollection<ScriptEditorViewModel> Scripts { get; set; }
        public BindableCollection<PanelViewModel> Tools { get; set; }

        public ScriptEditorViewModel ScriptEditor { get; set; }
        public MainMenuViewModel Menu { get; set; }

        public override void CanClose(Action<bool> callback)
        {
            eventAggregator.Publish(new ExitingEvent());

            persistanceManager.Save();
            var layoutSerializer = new XmlLayoutSerializer(DockingManager);
            layoutSerializer.Serialize(dockingConfig);

            base.CanClose(callback);
        }

        public void Handle(ScriptDocumentAddedEvent message)
        {
            Scripts.Add(message.Document);
            message.Document.IsActive = true;
        }
    }
}
