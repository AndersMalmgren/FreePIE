using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Extensions;
using FreePIE.Core.Common.Lifetime;
using FreePIE.Core.Model.Events;
using FreePIE.Core.ScriptEngine;
using FreePIE.GUI.Common.Strategies;
using FreePIE.GUI.Events;
using FreePIE.GUI.Events.Command;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells;
using FreePIE.GUI.Shells.Curves;
using FreePIE.GUI.Views.Plugin;
using FreePIE.GUI.Views.Script;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Main
{
    public class MainMenuViewModel : PropertyChangedBase, 
        Core.Common.Events.IHandle<ScriptUpdatedEvent>, 
        Core.Common.Events.IHandle<ExitingEvent>, 
        Core.Common.Events.IHandle<ActiveScriptDocumentChangedEvent>,
        Core.Common.Events.IHandle<ScriptErrorEvent>,
        Core.Common.Events.IHandle<FileEvent>,
        Core.Common.Events.IHandle<RunEvent>
    {
        private readonly IResultFactory resultFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly Func<IScopedContext<IScriptEngine>> scriptEngineScopeFactory;
        private readonly Func<ScriptEditorViewModel> scriptEditorFactory;
        private readonly IFileSystem fileSystem;
        private readonly ScriptDialogStrategy scriptDialogStrategy;
		private IScopedContext<IScriptEngine> currentScriptEngineScope;
        private bool scriptRunning;

        public MainMenuViewModel(IResultFactory resultFactory, 
            IEventAggregator eventAggregator,
			Func<IScopedContext<IScriptEngine>> scriptEngineScopeFactory,
            Func<ScriptEditorViewModel> scriptEditorFactory,
            IFileSystem fileSystem,
            ScriptDialogStrategy scriptDialogStrategy)
        {
            eventAggregator.Subscribe(this);
           
            this.resultFactory = resultFactory;
            this.eventAggregator = eventAggregator;
            this.scriptEngineScopeFactory = scriptEngineScopeFactory;
            this.scriptEditorFactory = scriptEditorFactory;
            this.fileSystem = fileSystem;
            this.scriptDialogStrategy = scriptDialogStrategy;
        }

        private PanelViewModel activeDocument;
        private PanelViewModel ActiveDocument
        {
            get { return activeDocument; }
            set { 
                activeDocument = value; 
                NotifyOfPropertyChange(() => CanQuickSaveScript);
                NotifyOfPropertyChange(() => CanSaveScript);
                NotifyOfPropertyChange(() => CanRunScript);
            }
        }

        public void NewScript()
        {
            CreateScriptViewModel(null);
        }

        public IEnumerable<IResult> OpenScript()
        {
            return scriptDialogStrategy.Open(CreateScriptViewModel);
        }

        private void CreateScriptViewModel(string filePath)
        {
            var document = scriptEditorFactory()
                .Configure(filePath);

            if (!string.IsNullOrEmpty(filePath))
                document.LoadFileContent(fileSystem.ReadAllText(filePath));

            eventAggregator.Publish(new ScriptDocumentAddedEvent(document));
        }

        public IEnumerable<IResult> SaveScript()
        {
            return SaveScript(ActiveDocument);
        }

        public IEnumerable<IResult> SaveScript(PanelViewModel document)
        {
            return scriptDialogStrategy.SaveAs(document, false, path => Save(document, path));
        }

        private void Save(PanelViewModel document)
        {
            Save(document, document.FilePath);
        }

        private void Save(PanelViewModel document, string filePath)
        {
            document.FilePath = filePath;
            fileSystem.WriteAllText(filePath, document.FileContent);
            document.Saved();
        }

        public IEnumerable<IResult> QuickSaveScript()
        {
            if (PathSet)
            {
                Save(activeDocument);
                return null;
            }

            return SaveScript();
        }

        public bool CanQuickSaveScript
        {
            get { return CanSaveScript; }
        }

        public bool PathSet
        {
            get { return !string.IsNullOrEmpty(activeDocument.FilePath); }
        }

        public bool CanSaveScript
        {
            get { return activeDocument != null; }
        }

        public void RunScript()
        {
            scriptRunning = true;
            PublishScriptStateChange();

            currentScriptEngineScope = scriptEngineScopeFactory();
            currentScriptEngineScope.EntryPoint.Start(activeDocument.FileContent);
        }

        public void StopScript()
        {
            scriptRunning = false;
            currentScriptEngineScope.EntryPoint.Stop();
            PublishScriptStateChange();
        }

        public IEnumerable<IResult> ShowAbout()
        {
            yield return resultFactory.ShowDialog<AboutViewModel>();
        }

        private void PublishScriptStateChange()
        {
            NotifyOfPropertyChange(() => CanRunScript);
            NotifyOfPropertyChange(() => CanStopScript);
            eventAggregator.Publish(new ScriptStateChangedEvent(scriptRunning));
        }

        public bool CanStopScript
        {
            get { return scriptRunning; }
        }

        public bool CanRunScript
        {
            get { return !scriptRunning && activeDocument != null && !string.IsNullOrEmpty(activeDocument.FileContent); }
        }

        public void Handle(FileEvent message)
        {
            CreateScriptViewModel(message.Data);
        }

        public void Handle(RunEvent message)
        {
            if(CanRunScript)
                RunScript();
        }

        public void Handle(ScriptUpdatedEvent message)
        {
            NotifyOfPropertyChange(() => CanRunScript);
        }

        public void Handle(ExitingEvent message)
        {
            if(scriptRunning)
                StopScript();
        }

        public void Handle(ActiveScriptDocumentChangedEvent message)
        {
            ActiveDocument = message.Document;
        }

        public void Handle(ScriptErrorEvent message)
        {
            if (message.Level == ErrorLevel.Exception)
            {
                scriptRunning = false;
                PublishScriptStateChange();
            }
        }

        public IEnumerable<IResult> Close()
        {
            yield return resultFactory.Close();
        }

        public IEnumerable<IResult> ShowCurveSettingsMenu()
        {
            yield return resultFactory.ShowDialog<CurveSettingsViewModel>();
        }

        public IEnumerable<IResult> ShowPluginSettings(PluginSettingsMenuViewModel pluginMenu)
        {
            yield return resultFactory.ShowDialog<PluginSettingsViewModel>()
                .Configure(p => p.Init(pluginMenu.PluginSetting));
        }

        public IEnumerable<IResult> ShowPluginHelp(PluginHelpFileViewModel pluginMenu)
        {
            yield return resultFactory.ShowDialog<PluginHelpViewModel>()
                .Configure(p => p.Init(pluginMenu.PluginSetting));
        }

        public void ShowView(PanelViewModel panel)
        {
            panel.IsVisible = true;
            panel.IsActive = true;
        }

        public IEnumerable<PluginSettingsMenuViewModel> Plugins { get; set; }
        public IEnumerable<PluginHelpFileViewModel> HelpFiles { get; set; }
        public IEnumerable<PanelViewModel> Views { get; set; }
    }
}
