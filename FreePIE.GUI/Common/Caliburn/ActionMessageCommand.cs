using System;
using System.Windows.Input;
using Caliburn.Micro;

namespace FreePIE.GUI.Common.Caliburn
{
    public class ActionMessageCommand : ActionMessage, ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
