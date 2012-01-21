using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace FreePIE.GUI.Result
{
    public class FileDialogResult : Result
    {
        private readonly string title;
        private readonly string filter;

        public FileDialogResult(string title, string filter)
        {
            this.title = title;
            this.filter = filter;
        }

        public string File { get; private set; }

        public override void Execute(Caliburn.Micro.ActionExecutionContext context)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = title;
            dialog.Filter = filter;

            dialog.ShowDialog();
            File = dialog.FileName;

            base.Execute(context);
        }
    }
}
