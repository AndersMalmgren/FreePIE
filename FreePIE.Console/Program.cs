using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using Ninject;
using FreePIE.Core.Services;
using FreePIE.Core.ScriptEngine;
using FreePIE.Core.Persistence;

namespace FreePIE.Console {

   class Program {

      static ManualResetEvent ExitLock = new ManualResetEvent(false);
      static LuaEngine s_ParserEngine = null;
      

      //-----------------------------------------------------------------------
      static void CtrlCHandler(object sender, ConsoleCancelEventArgs args) {
         // Called by a system thread when the Ctrl-C is caught
         // Gracefully shuts down the parser engine and allows the app to complete
         ShutDown();
      }

      //-----------------------------------------------------------------------
      static void ShutDown() {
         if (s_ParserEngine != null) {
            System.Console.WriteLine("Stopping script parser");
            s_ParserEngine.Stop();

            ExitLock.Set();  // Allow the main thread to finish
         }
      }

      //-----------------------------------------------------------------------
      static void PrintHelp() {

         System.Console.WriteLine("FreePIE.exe <script_file>");
      }
     
      //-----------------------------------------------------------------------
      static void OnScriptEngineError(object sender, ScriptErrorEventArgs e) {
         
         System.Console.WriteLine(e.Exception);
         ShutDown();
      }

      //-----------------------------------------------------------------------
      static void Main(string[] args) {
         
         // Parse the command line
         if (args.Length < 1) {
            PrintHelp();
            return;
         }
         
         String script = null;
         try {
            script = System.IO.File.ReadAllText(args[0]);
         }
         catch (Exception err) {
            System.Console.WriteLine("Can't open script file");
            return;
         }

         // Set up a Ctrl-C event handler to clean up when user kills the process
         System.Console.TreatControlCAsInput = false;
         
         System.Console.CancelKeyPress += new ConsoleCancelEventHandler(CtrlCHandler);

         // Generate all the binding between interfaces and class definitions
         IKernel kernel = ServiceBootstrapper.Create();

         // Instantiate the persistance (settings) scaffolding
         IEnumerable<object> services = kernel.GetAll(typeof(PersistanceManager));
         IEnumerator<object> e = (IEnumerator<object>)services.GetEnumerator();
         if (e.MoveNext()) {
            // Load the default settings
            PersistanceManager persistance = (PersistanceManager)e.Current;
            persistance.Load();
         }

         // Instantiate the parser
         services = kernel.GetAll(typeof(LuaEngine));
         e = (IEnumerator<object>)services.GetEnumerator();
         if (e.MoveNext()) {
            s_ParserEngine = (LuaEngine)e.Current;
         
            // Finally we can run the script parser
            System.Console.WriteLine("Starting script parser");
            s_ParserEngine.Start(script);
            s_ParserEngine.Error += OnScriptEngineError;

            // The script parser launches in a worker thread so the main thread will just wait here
            // until the stop command is called in the Ctrl-C handler
            ExitLock.WaitOne();
         }

         System.Console.WriteLine("Exiting");  // Hmmm...Need to investigate why I'm not getting this print?
      }
   }
}
