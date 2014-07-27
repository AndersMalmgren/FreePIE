using Caliburn.Micro;
using Microsoft.Win32;

namespace FreePIE.GUI.Result
{
    public class FileDialogResult : Result
    {
        private readonly string title;
        private readonly string filter;
        private readonly string fileName;

        public FileDialogResult(string title, string filter, FileDialogMode mode, string fileName)
        {
            Mode = mode;
            this.title = title;
            this.filter = filter;
            this.fileName = fileName;
        }

        public FileDialogMode Mode { get; private set; }
        public string File { get; private set; }
        public override void Execute(CoroutineExecutionContext context)
        {
            var dialog = Mode == FileDialogMode.Open ? new OpenFileDialog() as FileDialog : new SaveFileDialog();
            dialog.FileName = fileName;
            dialog.Title = title;
            dialog.Filter = filter;

            dialog.ShowDialog();
            File = dialog.FileName;

            base.Execute(context);
        }
    }

    public enum FileDialogMode
    {
        Open = 0,
        Save = 1
    }
}
