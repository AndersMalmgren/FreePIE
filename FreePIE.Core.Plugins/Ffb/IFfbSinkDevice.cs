namespace FreePIE.Core.Plugins.Ffb
{
    // Each source bridged to a device, and the script's own effects, get a channel of their own
    // so that effect indices and resets from one don't affect the others
    public interface IFfbSinkDevice
    {
        IFfbSink CreateChannel();
        void ReleaseChannel(IFfbSink channel);
        double Gain { get; set; }
    }
}
