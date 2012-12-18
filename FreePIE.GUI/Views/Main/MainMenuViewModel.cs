﻿using System;
using System.Collections.Generic;
using Caliburn.Micro;
using FreePIE.Core.Common;
using FreePIE.Core.ScriptEngine;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells;
using FreePIE.GUI.Views.Plugin;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Main
{
    public class MainMenuViewModel : PropertyChangedBase, Core.Common.Events.IHandle<ScriptUpdatedEvent>, Core.Common.Events.IHandle<ExitingEvent>
    {
        private readonly IResultFactory resultFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly Func<IScriptEngine> scriptEngineFactory;
        private readonly IFileSystem fileSystem;
        private IScriptEngine currentScriptEngine;
        private bool scriptRunning;

        public MainMenuViewModel(IResultFactory resultFactory, 
            IEventAggregator eventAggregator,
            Func<IScriptEngine> scriptEngineFactory,
            IFileSystem fileSystem)
        {
            eventAggregator.Subscribe(this);
           
            this.resultFactory = resultFactory;
            this.eventAggregator = eventAggregator;
            this.scriptEngineFactory = scriptEngineFactory;
            this.fileSystem = fileSystem;
        }

        private string currentScriptFile;
        private const string fileFilter = "Python scripts (*.py)|*.py|All files (*.*)|*.*";
        public IEnumerable<IResult> OpenScript()
        {
            var result = resultFactory.ShowFileDialog("Open script", fileFilter, FileDialogMode.Open);
            yield return result;

            if(!string.IsNullOrEmpty(result.File))
            {
                currentScriptFile = result.File;
                eventAggregator.Publish(new ScriptLoadedEvent(fileSystem.ReadAllText(result.File)));
                NotifyOfPropertyChange(() => CanQuickSaveScript);
            }
        }

        public IEnumerable<IResult> SaveScript()
        {
            var result = resultFactory.ShowFileDialog("Save script", fileFilter, FileDialogMode.Save, currentScriptFile);
            yield return result;

            if(!string.IsNullOrEmpty(result.File))
                Save(result.File);
        }

        private void Save(string filename)
        {
            currentScriptFile = filename;
            fileSystem.WriteAllText(filename, script);
            NotifyOfPropertyChange(() => CanQuickSaveScript);
        }

        public IEnumerable<IResult> QuickSaveScript()
        {
            if (CanQuickSaveScript)
            {
                Save(currentScriptFile);
                return null;
            }

            return SaveScript();
        }

        private bool CanQuickSaveScript
        {
            get { return !string.IsNullOrEmpty(currentScriptFile); }
        }

        public void RunScript()
        {
            scriptRunning = true;

            currentScriptEngine = scriptEngineFactory();
            currentScriptEngine.Error += ScriptEngineError;
            CanRunScript = !scriptRunning;
            CanStopScript = scriptRunning;
            currentScriptEngine.Start(script);

            PublishScriptStateChange();
        }

        public void StopScript()
        {
            scriptRunning = false;

            CanRunScript = !scriptRunning;
            CanStopScript = scriptRunning;
            currentScriptEngine.Stop();

            PublishScriptStateChange();
        }

        public IEnumerable<IResult> ShowAbout()
        {
            yield return resultFactory.ShowDialogResult<AboutViewModel>();
        }

        private void PublishScriptStateChange()
        {
            eventAggregator.Publish(new ScriptStateChangedEvent(scriptRunning));
        }

        private void ScriptEngineError(object sender, ScriptErrorEventArgs e)
        {
            StopScript();
            eventAggregator.Publish(new ScriptErrorEvent(e.Exception));
        }

        private bool canStopScript;
        public bool CanStopScript
        {
            get { return canStopScript; }
            set
            { 
                canStopScript = value; 
                NotifyOfPropertyChange(() => CanStopScript);
            }
        }

        private bool canRunScript;
        private string script;

        public bool CanRunScript
        {
            get { return canRunScript; }
            set 
            {
                canRunScript = value; 
                NotifyOfPropertyChange(() => CanRunScript);
            }
        }

        public void Handle(ScriptUpdatedEvent message)
        {
            CanRunScript = message.Script.Length > 0;
            script = message.Script;
        }

        public void Handle(ExitingEvent message)
        {
            if(scriptRunning)
                StopScript();
        }

        public IEnumerable<IResult> Close()
        {
            yield return resultFactory.Close();
        }

        public IEnumerable<IResult> ShowCurveSettingsMenu()
        {
            yield return resultFactory.ShowDialogResult<CurveSettingsViewModel>();
        }

        public IEnumerable<IResult> ShowPluginSettings(PluginSettingsMenuViewModel pluginMenu)
        {
            yield return resultFactory.ShowDialogResult<PluginSettingsViewModel>()
                .Configure(p => p.Init(pluginMenu.PluginSetting));
        }

        public IEnumerable<IResult> ShowPluginHelp(PluginHelpFileViewModel pluginMenu)
        {
            yield return resultFactory.ShowDialogResult<PluginHelpViewModel>()
                .Configure(p => p.Init(pluginMenu.PluginSetting));
        }

        public IEnumerable<PluginSettingsMenuViewModel> Plugins { get; set; }
        public IEnumerable<PluginHelpFileViewModel> HelpFiles { get; set; }
    }
}
