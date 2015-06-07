using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using FreePIE.GUI.Shells;
using StructureMap;

namespace FreePIE.GUI.Result
{
    public class ResultFactory : IResultFactory
    {
        private readonly IContainer container;

		public ResultFactory(IContainer container)
        {
			this.container = container;
        }

        public ShowDialogResult<TModel> ShowDialog<TModel>() where TModel : ShellPresentationModel
        {
			return container.GetInstance<ShowDialogResult<TModel>>();
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
			return new CloseResult();
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
