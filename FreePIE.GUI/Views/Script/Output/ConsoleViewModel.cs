using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using Caliburn.Micro;

namespace FreePIE.GUI.Views.Script.Output
{
    public class ConsoleViewModel : PropertyChangedBase
    {
        private readonly ConsoleTextWriter consoleTextWriter;

        public ConsoleViewModel()
        {
            consoleTextWriter = new ConsoleTextWriter(this);
            Console.SetOut(consoleTextWriter);
        }

        public void Clear()
        {
            consoleTextWriter.Clear();
        }

        private string text;
        public string Text
        {
            get { return text; }
            set 
            { 
                text = value; 
                NotifyOfPropertyChange(() => Text);
            }
        }
    }

    public class ConsoleTextWriter : TextWriter
    {
        private readonly ConsoleViewModel output;
        private string currentText;

        public ConsoleTextWriter(ConsoleViewModel output)
        {
            this.output = output;
            var worker = new BackgroundWorker();

            worker.DoWork += WorkerDoWork;
            worker.RunWorkerAsync();
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                output.Text = currentText;
                Thread.Sleep(500);
            }
        }

        public void Clear()
        {
            output.Text = null;
            currentText = null;
        }

        public override void WriteLine(string value)
        {
            currentText += value + "\r\n";
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
