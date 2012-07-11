namespace FreePIE.GUI.CodeCompletion.Event
{
    public interface IPopupEvent
    {
        EventType Type { get; }
        object EventArgs { get; }
    }
}