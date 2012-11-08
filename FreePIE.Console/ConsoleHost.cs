using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Model.Events;
using FreePIE.Core.Persistence;
using FreePIE.Core.ScriptEngine;

namespace FreePIE.Console
{
    public class ConsoleHost : IHandle<WatchEvent>
    {
        private readonly IScriptEngine scriptEngine;
        private readonly IPersistanceManager persistanceManager;
        private readonly IFileSystem fileSystem;
        private readonly IEventAggregator eventAggregator;
        private readonly AutoResetEvent waitUntilStopped;

        public ConsoleHost(IScriptEngine scriptEngine, IPersistanceManager persistanceManager, IFileSystem fileSystem, IEventAggregator eventAggregator)
        {
            this.scriptEngine = scriptEngine;
            this.persistanceManager = persistanceManager;
            this.fileSystem = fileSystem;
            this.eventAggregator = eventAggregator;
            waitUntilStopped = new AutoResetEvent(false);

            eventAggregator.Subscribe(this);
        }

        public void Start(string[] args)
        {

            try
            {
                string script = null;


                if (args.Length == 0) {
                    PrintHelp();
                    return;
                }

                try
                {
                    script = fileSystem.ReadAllText(args[0]);
                }
                catch (IOException e)
                {
                    System.Console.WriteLine("Can't open script file");
                    throw;
                }

                System.Console.TreatControlCAsInput = false;
                System.Console.CancelKeyPress += (s, e) => Stop();

                persistanceManager.Load();

                scriptEngine.Error += ScriptEngineError;
                System.Console.WriteLine("Starting script parser");
                scriptEngine.Start(script);
                waitUntilStopped.WaitOne();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        private void Stop()
        {
            System.Console.WriteLine("Stopping script parser");
            scriptEngine.Stop();

            persistanceManager.Save();
            waitUntilStopped.Set();
        }

        private void ScriptEngineError(object sender, ScriptErrorEventArgs e)
        {
            System.Console.WriteLine(e.Exception);
            Stop();
        }

        public void Handle(WatchEvent message)
        {
            System.Console.WriteLine("{0}: {1}", message.Name, message.Value);
        }

        private void PrintHelp()
        {
            System.Console.WriteLine("FreePIE.Console.exe <script_file>");
        }

    }
}
