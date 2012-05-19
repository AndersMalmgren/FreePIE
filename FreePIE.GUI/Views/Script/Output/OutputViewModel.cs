using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FreePIE.GUI.Views.Script.Output
{
    public class OutputViewModel : PropertyChangedBase
    {
        public OutputViewModel(ConsoleViewModel consoleViewModel, ErrorViewModel errorViewModel, WatchesViewModel watchViewModel)
        {
            Console = consoleViewModel;
            Error = errorViewModel;
            Watch = watchViewModel;
        }
        public ConsoleViewModel Console { get; set; }
        public ErrorViewModel Error { get; set; }
        public WatchesViewModel Watch { get; set; }
    }
}
