using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
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

        public ShowDialogResult<TModel> ShowDialog<TModel>() where TModel : ShellPresentationModel
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

        public IResult Cancel()
        {
            return new CancelResult();
        }

        public IResult Close()
        {
            return kernel.Get<CloseResult>();
        }

        public IEnumerable<IResult> Coroutinify(IEnumerable<IResult> results, System.Action cancelCallback)
        {
            return results.Select(r =>
                {
                    if (r is CancelResult)
                        cancelCallback();

                    return r;
                });
        }
    }
}
