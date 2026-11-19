namespace FreePIE.Core.Plugins.Ffb
{
    public interface IFfbSink
    {
        void SetEffect(int index, FfbEffect effect);
        void StartEffect(int index, int loopCount, bool solo);
        void StopEffect(int index);
        void RemoveEffect(int index);
        void SetGain(double gain);
        void Control(FfbControl control);
    }
}
