using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FreePIE.GUI.Views.Script.Output
{
    public class OutputViewModel : PropertyChangedBase
    {
        public OutputViewModel(ConsoleViewModel consoleViewModel, ErrorViewModel errorViewModel)
        {
            Console = consoleViewModel;
            Error = errorViewModel;
        }
        public ConsoleViewModel Console { get; set; }
        public ErrorViewModel Error { get; set; }
    }
}
