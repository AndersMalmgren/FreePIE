namespace FreePIE.GUI.CodeCompletion.Event
{
    public interface IPopupEvent
    {
        EventType Type { get; }
        EventSource Source { get; }
        object EventArgs { get; }
    }
}