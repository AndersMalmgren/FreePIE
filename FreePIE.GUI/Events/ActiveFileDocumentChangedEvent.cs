using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.GUI.Views.Main;

namespace FreePIE.GUI.Events
{
    public class ActiveFileDocumentChangedEvent
    {
        public PanelViewModel Document { get; private set; }
        public ActiveFileDocumentChangedEvent(PanelViewModel document)
        {
            Document = document;
        }
    }
}
