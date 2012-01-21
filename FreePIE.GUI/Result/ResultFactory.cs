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

        public FileDialogResult ShowFileDialog(string title, string filter)
        {
            return new FileDialogResult(title, filter);
        }

        public CloseResult Close()
        {
            return kernel.Get<CloseResult>();
        }
    }
}
