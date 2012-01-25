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
using FreePIE.GUI.Views.Script;
using FreePIE.GUI.Views.Script.Output;

namespace FreePIE.GUI.Shells
{
    public class MainShellViewModel : ShellPresentationModel, IHandle<ScriptUpdatedEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly Func<IScriptEngine> scriptEngineFactory;
        private readonly IPersistanceManager persistanceManager;
        private IScriptEngine currentSCriptEngine;
        private bool scriptRunning;

        public MainShellViewModel(IResultFactory resultFactory, 
            IEventAggregator eventAggregator, 
            Func<IScriptEngine> scriptEngineFactory, 
            IPersistanceManager persistanceManager,
            ScriptEditorViewModel scriptEditorViewModel,
            OutputViewModel outputViewModel)
            : base(resultFactory)
        {
            eventAggregator.Subscribe(this);

            this.eventAggregator = eventAggregator;
            this.scriptEngineFactory = scriptEngineFactory;
            this.persistanceManager = persistanceManager;
            persistanceManager.Load();

            ScriptEditor = scriptEditorViewModel;
            Output = outputViewModel;
            DisplayName = "FreePIE - Programmable Input Emulator";
        }

        private string currentScriptFile;
        private const string fileFilter = "Lua scripts (*.lua)|*.lua|All files (*.*)|*.*";
        public IEnumerable<IResult> OpenScript()
        {
            var result = Result.ShowFileDialog("Open script", fileFilter, FileDialogMode.Open);
            yield return result;

            if(!string.IsNullOrEmpty(result.File))
            {
                currentScriptFile = result.File;
                ScriptEditor.Script = File.ReadAllText(result.File);
                NotifyOfPropertyChange(() => CanQuickSaveScript);
            }
        }

        public IEnumerable<IResult> SaveScript()
        {
            var result = Result.ShowFileDialog("Save script", fileFilter, FileDialogMode.Save, currentScriptFile);
            yield return result;

            if(!string.IsNullOrEmpty(result.File))
                Save(result.File);
        }

        private void Save(string filename)
        {
            currentScriptFile = filename;
            File.WriteAllText(filename, ScriptEditor.Script);
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

            currentSCriptEngine = scriptEngineFactory();
            currentSCriptEngine.Error += new EventHandler<ScriptErrorEventArgs>(ScriptEngineError);
            CanRunScript = !scriptRunning;
            CanStopScript = scriptRunning;
            currentSCriptEngine.Start(ScriptEditor.Script);
        }

        public void StopScript()
        {
            scriptRunning = false;

            CanRunScript = !scriptRunning;
            CanStopScript = scriptRunning;
            currentSCriptEngine.Stop();
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
        }

        public ScriptEditorViewModel ScriptEditor { get; set; }
        public OutputViewModel Output { get; set; }

        public override void CanClose(Action<bool> callback)
        {
            if(scriptRunning)
                StopScript();

            persistanceManager.Save();
            base.CanClose(callback);
        }
    }
}
