using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.Core.Persistence;
using FreePIE.Core.ScriptEngine;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells;
using FreePIE.GUI.Views.Plugin;
using FreePIE.GUI.Views.Script;
using FreePIE.GUI.Views.Script.Output;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Main
{
    public class MainMenuViewModel : PropertyChangedBase, Core.Common.Events.IHandle<ScriptUpdatedEvent>, Core.Common.Events.IHandle<ExitingEvent>
    {
        private readonly IResultFactory resultFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly Func<IScriptEngine> scriptEngineFactory;
        private IScriptEngine currentScriptEngine;
        private bool scriptRunning;

        public MainMenuViewModel(IResultFactory resultFactory, 
            IEventAggregator eventAggregator, 
            Func<IScriptEngine> scriptEngineFactory)
        {
            eventAggregator.Subscribe(this);
           
            this.resultFactory = resultFactory;
            this.eventAggregator = eventAggregator;
            this.scriptEngineFactory = scriptEngineFactory;
        }

        private string currentScriptFile;
        private const string fileFilter = "Lua scripts (*.lua)|*.lua|All files (*.*)|*.*";
        public IEnumerable<IResult> OpenScript()
        {
            var result = resultFactory.ShowFileDialog("Open script", fileFilter, FileDialogMode.Open);
            yield return result;

            if(!string.IsNullOrEmpty(result.File))
            {
                currentScriptFile = result.File;
                eventAggregator.Publish(new ScriptLoadedEvent(File.ReadAllText(result.File)));
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
            File.WriteAllText(filename, script);
            NotifyOfPropertyChange(() => CanQuickSaveScript);
        }

        public void QuickSaveScript()
        {
            Save(currentScriptFile);
        }

        public bool CanQuickSaveScript
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

        public IEnumerable<PluginSettingsMenuViewModel> Plugins { get; set; }
    }
}
