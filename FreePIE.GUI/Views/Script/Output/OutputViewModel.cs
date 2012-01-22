using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FreePIE.GUI.Views.Script.Output
{
    public class OutputViewModel : PropertyChangedBase
    {
        public OutputViewModel(ConsoleViewModel consoleViewModel)
        {
            Console = consoleViewModel;
        }
        public ConsoleViewModel Console { get; set; }
    }
}
