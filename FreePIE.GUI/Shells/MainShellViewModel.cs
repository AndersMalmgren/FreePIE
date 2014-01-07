using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using FreePIE.Core.Common;
using FreePIE.Core.Model;
using FreePIE.Core.Persistence;
using FreePIE.GUI.Common.AvalonDock;
using FreePIE.GUI.Common.Strategies;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Views.Main;
using FreePIE.GUI.Views.Plugin;
using FreePIE.GUI.Views.Script;
using FreePIE.GUI.Views.Script.Output;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Action = System.Action;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;
using MessageBoxResult = System.Windows.MessageBoxResult;
//using Caliburn.Micro;

namespace FreePIE.GUI.Shells
{
    public class MainShellViewModel : ShellPresentationModel, Core.Common.Events.IHandle<ScriptDocumentAddedEvent>
    {
        private const string dockingConfig = "layout.config";
        private readonly IEventAggregator eventAggregator;
        private readonly IPersistanceManager persistanceManager;
        private readonly ISettingsManager settingsManager;
        private readonly IFileSystem fileSystem;
        private readonly ScriptDialogStrategy scriptDialogStrategy;
        private readonly IPaths paths;
        private readonly ViewBag viewBag;


        public MainShellViewModel(IResultFactory resultFactory,
                                  IEventAggregator eventAggregator,
                                  IPersistanceManager persistanceManager,
                                  ISettingsManager settingsManager,
                                  MainMenuViewModel mainMenuViewModel,
                                  ConsoleViewModel consoleViewModel,
                                  ErrorViewModel errorViewModel,
                                  WatchesViewModel watchesViewModel,
                                  IFileSystem fileSystem,
                                  ScriptDialogStrategy scriptDialogStrategy,
                                  IPaths paths
            )
            : base(resultFactory)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.persistanceManager = persistanceManager;
            this.fileSystem = fileSystem;
            this.scriptDialogStrategy = scriptDialogStrategy;
            this.paths = paths;
            viewBag = settingsManager.Settings.ViewBag;

            Scripts = new BindableCollection<ScriptEditorViewModel>();
            Tools = new BindableCollection<PanelViewModel> {consoleViewModel, errorViewModel, watchesViewModel};

            Menu = mainMenuViewModel;
            Menu.Plugins =
                settingsManager.ListConfigurablePluginSettings().Select(ps => new PluginSettingsMenuViewModel(ps));
            Menu.HelpFiles =
                settingsManager.ListPluginSettingsWithHelpFile().Select(ps => new PluginHelpFileViewModel(ps)).ToList();
            Menu.Views = Tools;

            DisplayName = "FreePIE - Programmable Input Emulator";
            LoadPersistedViewState();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            InitDocking();
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

        private void LoadPersistedViewState()
        {
            Top = viewBag.Get<double?>("top");
            Left = viewBag.Get<double?>("left");
            Width = viewBag.Get<double?>("width");
            Height = viewBag.Get<double?>("height");
            State = (WindowState)viewBag.Get<int>("state");
        }

        private void PersistViewState()
        {
            viewBag.Set("top", Top);
            viewBag.Set("left", Left);
            viewBag.Set("width", Width);
            viewBag.Set("height", Height);
            viewBag.Set("state", (int)State);
        }
        
        public BindableCollection<ScriptEditorViewModel> Scripts { get; set; }
        public BindableCollection<PanelViewModel> Tools { get; set; }
        public MainMenuViewModel Menu { get; set; }

        protected override IEnumerable<IResult> CanClose()
        {
            var handleDirtyResults = Scripts.SelectMany(HandleScriptClosing);
            foreach (var result in handleDirtyResults)
            {
                yield return result;
            }

            eventAggregator.Publish(new ExitingEvent());

            PersistViewState();
            persistanceManager.Save();
            var layoutSerializer = new XmlLayoutSerializer(DockingManager);
            layoutSerializer.Serialize(paths.GetDataPath(dockingConfig));
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
        }

        public double? Top { get; set; }
        public double? Left { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public WindowState State { get; set; }
    }
}
