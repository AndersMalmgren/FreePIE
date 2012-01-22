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
        private readonly Func<IScriptEngine> scriptEngineFactory;
        private readonly IPersistanceManager persistanceManager;
        private IScriptEngine currentSCriptEngine;

        public MainShellViewModel(IResultFactory resultFactory, 
            IEventAggregator eventAggregator, 
            Func<IScriptEngine> scriptEngineFactory, 
            IPersistanceManager persistanceManager,
            ScriptEditorViewModel scriptEditorViewModel,
            OutputViewModel outputViewModel)
            : base(resultFactory)
        {
            eventAggregator.Subscribe(this);

            this.scriptEngineFactory = scriptEngineFactory;
            this.persistanceManager = persistanceManager;
            persistanceManager.Load();

            ScriptEditor = scriptEditorViewModel;
            Output = outputViewModel;
            DisplayName = "FreePIE - Programmable Input Emulator";
        }

        public IEnumerable<IResult> OpenScript()
        {
            var result = Result.ShowFileDialog("Open script", "Lua scripts (*.lua)|*.lua|All files (*.*)|*.*");
            yield return result;

            if(!string.IsNullOrEmpty(result.File))
            {
                ScriptEditor.Script = File.ReadAllText(result.File);
            }
        }

        public void RunScript()
        {
            currentSCriptEngine = scriptEngineFactory();
            CanRunScript = false;
            CanStopScript = true;
            currentSCriptEngine.Start(ScriptEditor.Script);
        }

        public void StopScript()
        {
            CanRunScript = true;
            CanStopScript = false;
            currentSCriptEngine.Stop();
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
            persistanceManager.Save();
            base.CanClose(callback);
        }
    }
}
