using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using Caliburn.Micro;

namespace FreePIE.GUI.Views.Script.Output
{
    public class ConsoleViewModel : PropertyChangedBase
    {
        public ConsoleViewModel()
        {
            Console.SetOut(new ConsoleTextWriter(this));
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
        private int lines;
        private string currentText;

        public ConsoleTextWriter(ConsoleViewModel output)
        {
            this.output = output;
            var worker = new BackgroundWorker();

            worker.DoWork += new DoWorkEventHandler(WorkerDoWork);
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

        public override void WriteLine(string value)
        {
            if (lines > 500)
            {
                currentText = string.Empty;
                lines = 0;
            }
            lines++;
            currentText += value + "\r\n";

            //newLines.Enqueue(value);
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
