using FreePIE.GUI.Views.Main;

namespace FreePIE.GUI.Events
{
    public class ActiveScriptDocumentChangedEvent
    {
        public PanelViewModel Document { get; private set; }
        public ActiveScriptDocumentChangedEvent(PanelViewModel document)
        {
            Document = document;
        }
    }
}
