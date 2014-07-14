﻿using System;
using System.IO;
using System.Threading;
using FreePIE.Core.Common;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Model.Events;
using FreePIE.Core.Persistence;
using FreePIE.Core.ScriptEngine;

namespace FreePIE.Console
{
    public class ConsoleHost : IHandle<WatchEvent>, IHandle<ScriptErrorEvent>
    {
        private readonly IScriptEngine scriptEngine;
        private readonly IPersistanceManager persistanceManager;
        private readonly IFileSystem fileSystem;
        private readonly AutoResetEvent waitUntilStopped;

        public ConsoleHost(IScriptEngine scriptEngine, IPersistanceManager persistanceManager, IFileSystem fileSystem, IEventAggregator eventAggregator)
        {
            this.scriptEngine = scriptEngine;
            this.persistanceManager = persistanceManager;
            this.fileSystem = fileSystem;
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
                catch (IOException)
                {
                    System.Console.WriteLine("Can't open script file");
                    throw;
                }

                System.Console.TreatControlCAsInput = false;
                System.Console.CancelKeyPress += (s, e) => Stop();

                persistanceManager.Load();

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

        public void Handle(WatchEvent message)
        {
            System.Console.WriteLine("{0}: {1}", message.Name, message.Value);
        }

        public void Handle(ScriptErrorEvent message)
        {
            System.Console.WriteLine(message.Description);
            if(message.Level == ErrorLevel.Exception)
                Stop();
        }

        private void PrintHelp()
        {
            System.Console.WriteLine("FreePIE.Console.exe <script_file>");
        }
    }
}
