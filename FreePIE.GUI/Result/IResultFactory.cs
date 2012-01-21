using FreePIE.GUI.Shells;

namespace FreePIE.GUI.Result
{
    public interface IResultFactory
    {
        ShowDialogResult<TModel> ShowDialogResult<TModel>() where TModel : ShellPresentationModel;
        CloseResult Close();
        FileDialogResult ShowFileDialog(string title, string filter);
    }
}