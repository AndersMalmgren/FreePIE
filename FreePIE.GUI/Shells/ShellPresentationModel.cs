using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.GUI.Result;

namespace FreePIE.GUI.Shells
{
    public abstract class ShellPresentationModel : Screen
    {
        public ShellPresentationModel(IResultFactory resultFactory)
        {
            Result = resultFactory;
        }
        
        public IResultFactory Result { get; private set; }
    }
}
