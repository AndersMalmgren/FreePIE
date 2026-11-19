namespace FreePIE.Core.Plugins.Ffb
{
    public interface IFfbSource
    {
        void AddSink(IFfbSink sink);
        void RemoveSink(IFfbSink sink);
    }
}
