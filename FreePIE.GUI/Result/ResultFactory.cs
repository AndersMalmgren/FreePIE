using System.Windows;
using FreePIE.GUI.Shells;
using Ninject;

namespace FreePIE.GUI.Result
{
    public class ResultFactory : IResultFactory
    {
        private readonly IKernel kernel;

        public ResultFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public ShowDialogResult<TModel> ShowDialogResult<TModel>() where TModel : ShellPresentationModel
        {
            return kernel.Get<ShowDialogResult<TModel>>();
        }

        public FileDialogResult ShowFileDialog(string title, string filter, FileDialogMode mode)
        {
            return ShowFileDialog(title, filter, mode, null);
        }

        public FileDialogResult ShowFileDialog(string title, string filter, FileDialogMode mode, string fileName)
        {
            return new FileDialogResult(title, filter, mode, fileName);
        }

        public MessageBoxResult ShowMessageBox(string caption, string text, MessageBoxButton buttons)
        {
            return new MessageBoxResult(caption, text, buttons);
        }

        public CloseResult Close()
        {
            return kernel.Get<CloseResult>();
        }
    }
}
