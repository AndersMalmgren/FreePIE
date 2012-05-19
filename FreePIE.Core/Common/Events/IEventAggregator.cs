namespace FreePIE.Core.Common.Events
{
    public interface IEventAggregator
    {
        void Subscribe(object subsriber);
        void Publish<T>(T message) where T : class;
    }
}
