using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using FreePIE.Core.Common;
using FreePIE.Core.Persistence;
using FreePIE.Core.Persistence.Paths;
using FreePIE.GUI.Common.AvalonDock;
using FreePIE.GUI.Common.CommandLine;
using FreePIE.GUI.Common.Strategies;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Views.Main;
using FreePIE.GUI.Views.Plugin;
using FreePIE.GUI.Views.Script;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace FreePIE.GUI.Shells
{
    public class MainShellViewModel : ShellPresentationModel, 
        Core.Common.Events.IHandle<ScriptDocumentAddedEvent>
        ,Core.Common.Events.IHandle<ExitingEvent>
    {
        private const string dockingConfig = "layout.config";
        private readonly IEventAggregator eventAggregator;
        private readonly IPersistanceManager persistanceManager;
        private readonly ISettingsManager settingsManager;
        private readonly IFileSystem fileSystem;
        private readonly ScriptDialogStrategy scriptDialogStrategy;
        private readonly IPaths paths;
        private readonly IParser parser;
        private WindowState windowState = WindowState.Minimized;
        private bool showInTaskBar = true;
        public MainShellViewModel(IResultFactory resultFactory,
                                  IEventAggregator eventAggregator,
                                  IPersistanceManager persistanceManager,
                                  ISettingsManager settingsManager,
                                  MainMenuViewModel mainMenuViewModel,
                                  IEnumerable<PanelViewModel> panels,
                                  IFileSystem fileSystem,
                                  ScriptDialogStrategy scriptDialogStrategy,
                                  IPaths paths,
                                  IParser parser,
                                  IPortable portable
            )
            : base(resultFactory)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.persistanceManager = persistanceManager;
            this.settingsManager = settingsManager;
            this.fileSystem = fileSystem;
            this.scriptDialogStrategy = scriptDialogStrategy;
            this.paths = paths;
            this.parser = parser;

            Scripts = new BindableCollection<ScriptEditorViewModel>();
            Tools = new BindableCollection<PanelViewModel> (panels);
            
            Menu = mainMenuViewModel;
            Menu.Plugins =
                settingsManager.ListConfigurablePluginSettings().Select(ps => new PluginSettingsMenuViewModel(ps));
            Menu.HelpFiles =
                settingsManager.ListPluginSettingsWithHelpFile().Select(ps => new PluginHelpFileViewModel(ps)).ToList();
            Menu.Views = Tools;

            DisplayName = string.Format("FreePIE - Programmable Input Emulator{0}", portable.IsPortable ? " (Portable mode)" : null);
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
            ShowInTaskBar = !settingsManager.Settings.MinimizeToTray;
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            InitDocking();
            parser.ParseAndExecute();
            eventAggregator.Publish(new StartedEvent());
        }

        private void InitDocking()
        {
            var path = paths.GetDataPath(dockingConfig);

            if (!fileSystem.Exists(path)) return;
            try
            {
                var layoutSerializer = new XmlLayoutSerializer(DockingManager);
                layoutSerializer.Deserialize(path);
            }
            catch
            {
                fileSystem.Delete(path);
            }
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
            return Result.Coroutinify(HandleScriptClosing(document), () => e.Cancel = true);
        }

        public void DocumentClosed(ScriptEditorViewModel document)
        {
            Scripts.Remove(document);
        }

        private IEnumerable<IResult> HandleScriptClosing(ScriptEditorViewModel script)
        {
            if (script.IsDirty)
            {
                var message = Result.ShowMessageBox(script.Filename, string.Format("Do you want to save changes to {0}", script.Filename), MessageBoxButton.YesNoCancel);
                yield return message;

                if (message.Result == MessageBoxResult.Cancel)
                {
                    yield return Result.Cancel();
                }
                else if (message.Result == MessageBoxResult.Yes)
                {
                    foreach (var result in scriptDialogStrategy.SaveAs(script, true, path => fileSystem.WriteAllText(path, script.FileContent)))
                        yield return result;
                }
            }
        } 

        public BindableCollection<ScriptEditorViewModel> Scripts { get; set; }
        public BindableCollection<PanelViewModel> Tools { get; set; }
        public MainMenuViewModel Menu { get; set; }
        public WindowState WindowState
        {
            get { return windowState; }
            set
            {
                if (value == windowState) return;
                windowState = value;
                if (value == WindowState.Minimized)
                    ShowInTaskBar = !settingsManager.Settings.MinimizeToTray;
                else
                    ShowInTaskBar = true;
                NotifyOfPropertyChange(() => WindowState);
            }
        }

        public bool ShowInTaskBar
        {
            get { return showInTaskBar; }
            set
            {
                if (value == showInTaskBar) return;
                    showInTaskBar = value;
                
                NotifyOfPropertyChange(() => ShowInTaskBar);
            }
        }


        protected override IEnumerable<IResult> CanClose()
        {
            var handleDirtyResults = Scripts.SelectMany(HandleScriptClosing);
            foreach (var result in handleDirtyResults)
            {
                yield return result;
            }
                
            eventAggregator.Publish(new ExitingEvent());

        }
        

        public void Handle(ScriptDocumentAddedEvent message)
        {
            var script = Scripts.FirstOrDefault(s => s.ContentId == message.Document.ContentId);

            if (script == null)
            {
                script = message.Document;
                Scripts.Add(script);
            }

            script.IsActive = true;

            
            ActiveDocument = message.Document;
        }

        public void Handle(ExitingEvent message)
        {
            persistanceManager.Save();
            var layoutSerializer = new XmlLayoutSerializer(DockingManager);
            layoutSerializer.Serialize(paths.GetDataPath(dockingConfig));
        }
    }
}
