using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using FreePIE.GUI.Views.Main;

namespace FreePIE.GUI.Views.Script.Output
{
    public class ConsoleViewModel : PanelViewModel
    {
        private readonly ConsoleTextWriter consoleTextWriter;

        public ConsoleViewModel()
        {
            consoleTextWriter = new ConsoleTextWriter(this);
            Console.SetOut(consoleTextWriter);

            Title = "Console";
            IconName = "console-16.png";
        }

        public void Clear()
        {
            consoleTextWriter.Clear();
        }

        public bool CanClear { get { return !string.IsNullOrEmpty(Text); } }

        private string text;
        public string Text
        {
            get { return text; }
            set 
            { 
                text = value; 
                NotifyOfPropertyChange(() => Text);
                NotifyOfPropertyChange(() => CanClear);
            }
        }
    }

    public class ConsoleTextWriter : TextWriter
    {
        private readonly ConsoleViewModel output;
        private List<string> consoleStack;

        public ConsoleTextWriter(ConsoleViewModel output)
        {
            this.output = output;
            consoleStack = new List<string>();
            var worker = new BackgroundWorker();

            worker.DoWork += WorkerDoWork;
            worker.RunWorkerAsync();
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                if(consoleStack.Count > 0)
                    output.Text = string.Join(Environment.NewLine, consoleStack.ToArray());

                Thread.Sleep(100);
            }
        }

        public void Clear()
        {
            output.Text = null;
            consoleStack.Clear();
        }

        public override void WriteLine(string value)
        {
            consoleStack.Add(value);
            if (consoleStack.Count > 1000)
            {
                consoleStack.RemoveAt(0);
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
