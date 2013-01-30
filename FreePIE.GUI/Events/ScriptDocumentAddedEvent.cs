using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.GUI.Views.Main;
using FreePIE.GUI.Views.Script;

namespace FreePIE.GUI.Events
{
    public class ScriptDocumentAddedEvent
    {
        public ScriptEditorViewModel Document { get; private set; }
        public ScriptDocumentAddedEvent(ScriptEditorViewModel document)
        {
            Document = document;
        }
    }
}
