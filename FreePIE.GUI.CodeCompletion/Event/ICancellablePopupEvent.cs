namespace FreePIE.GUI.CodeCompletion.Event
{
    public interface ICancellablePopupEvent : IPopupEvent
    {
        void Cancel();
        bool IsCancelled { get; }
        bool IsTransient { get; }
    }
}