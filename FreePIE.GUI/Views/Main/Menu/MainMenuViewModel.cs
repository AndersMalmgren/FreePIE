﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using FreePIE.Core.Common;
using FreePIE.Core.Model.Events;
using FreePIE.Core.Persistence;
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

namespace FreePIE.GUI.Views.Main.Menu
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
        private readonly Func<IScriptEngine> scriptEngineFactory;
        private readonly Func<ScriptEditorViewModel> scriptEditorFactory;
        private readonly IFileSystem fileSystem;
        private readonly ScriptDialogStrategy scriptDialogStrategy;
        private readonly ISettingsManager settingsManager;
        private IScriptEngine currentScriptEngine;
        private bool scriptRunning;

        public MainMenuViewModel(IResultFactory resultFactory,
            IEventAggregator eventAggregator,
            Func<IScriptEngine> scriptEngineFactory,
            Func<ScriptEditorViewModel> scriptEditorFactory,
            IFileSystem fileSystem,
            ScriptDialogStrategy scriptDialogStrategy,
            ISettingsManager settings)
        {
            eventAggregator.Subscribe(this);

            this.resultFactory = resultFactory;
            this.eventAggregator = eventAggregator;
            this.scriptEngineFactory = scriptEngineFactory;
            this.scriptEditorFactory = scriptEditorFactory;
            this.fileSystem = fileSystem;
            this.scriptDialogStrategy = scriptDialogStrategy;
            this.settingsManager = settings;

            RecentScripts = new BindableCollection<RecentFileViewModel>(ListRecentFiles());
        }

        private PanelViewModel activeDocument;
        private PanelViewModel ActiveDocument
        {
            get { return activeDocument; }
            set {
                activeDocument = value;
                NotifyOfPropertyChange(() => CanQuickSaveScript);
                NotifyOfPropertyChange(() => CanSaveScript);
                NotifyOfPropertyChange(() => CanCloseScript);
                PublishScriptStateChange(false);
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

        public void CloseScript()
        {
            activeDocument.Close();
        }

        public void CreateScriptViewModel(string filePath)
        {
            if (filePath != null && !fileSystem.Exists(filePath)) return;

            var document = scriptEditorFactory()
                .Configure(filePath);

            if (!string.IsNullOrEmpty(filePath))
            {
                document.LoadFileContent(fileSystem.ReadAllText(filePath));
                AddRecentScript(filePath);
            }

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

            AddRecentScript(filePath);
        }

        public bool CanOpenRecentScript
        {
            get { return RecentScripts.Any(); }
        }

        public void OpenRecentScript(RecentFileViewModel model)
        {
            CreateScriptViewModel(model.File);
        }

        private void AddRecentScript(string filePath)
        {
            settingsManager.Settings.AddRecentScript(filePath);
            RecentScripts.Clear();
            RecentScripts.AddRange(ListRecentFiles());
            NotifyOfPropertyChange(() => CanOpenRecentScript);
        }

        private IEnumerable<RecentFileViewModel> ListRecentFiles()
        {
            return settingsManager.Settings.RecentScripts.Select((file, index) => new RecentFileViewModel(file, index));
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

        public bool CanQuickSaveScript => CanSaveScript;

        public bool CanCloseScript => activeDocument != null;

        public bool PathSet => !string.IsNullOrEmpty(activeDocument.FilePath);

        public bool CanSaveScript => activeDocument != null;

        public void RunScript()
        {
            scriptRunning = true;
            PublishScriptStateChange();

            currentScriptEngine = scriptEngineFactory();

            currentScriptEngine.Start(activeDocument.FileContent, activeDocument.FilePath);
        }

        public void StopScript()
        {
            scriptRunning = false;
            currentScriptEngine.Stop();
            PublishScriptStateChange();
        }

        public IEnumerable<IResult> ShowAbout()
        {
            yield return resultFactory.ShowDialog<AboutViewModel>();
        }

        private void PublishScriptStateChange(bool triggerScriptChangeEvent = true)
        {
            NotifyOfPropertyChange(() => CanRunScript);
            NotifyOfPropertyChange(() => CanStopScript);
            if(triggerScriptChangeEvent)
                eventAggregator.Publish(new ScriptStateChangedEvent(scriptRunning, activeDocument?.Filename));
        }

        public bool CanStopScript => scriptRunning;

        public bool CanRunScript => !scriptRunning && !string.IsNullOrEmpty(activeDocument?.FileContent);

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
            yield return resultFactory.CloseApp();
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

        public IObservableCollection<RecentFileViewModel> RecentScripts { get; set; }
    }
}
