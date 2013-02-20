using System.Windows;
using Caliburn.Micro;
using FreePIE.GUI.Shells;

namespace FreePIE.GUI.Result
{
    public interface IResultFactory
    {
        ShowDialogResult<TModel> ShowDialogResult<TModel>() where TModel : ShellPresentationModel;
        CloseResult Close();
        FileDialogResult ShowFileDialog(string title, string filter, FileDialogMode mode);
        FileDialogResult ShowFileDialog(string title, string filter, FileDialogMode mode, string fileName);
        MessageBoxResult ShowMessageBox(string caption, string text, MessageBoxButton buttons);
        IResult Cancel(System.Action cancelCallback);
    }
}