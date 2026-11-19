using System.Linq;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Ffb
{
    [Documentation("Force feedback sent by games to an emulated device, bridge it to a device with force feedback or read the effects")]
    public class FfbSourceGlobal : IFfbSource
    {
        private readonly FfbSource source;
        private event GlobalNoArgumentEvent onUpdate;

        public FfbSourceGlobal(FfbSource source)
        {
            this.source = source;
        }

        internal void Pump()
        {
            if (source.Pump() && onUpdate != null)
                onUpdate();
        }

        void IFfbSource.AddSink(IFfbSink sink)
        {
            source.AddSink(sink);
        }

        void IFfbSource.RemoveSink(IFfbSink sink)
        {
            source.RemoveSink(sink);
        }

        [Documentation("Raised when the game has changed the force feedback, assign a function with +=")]
        public event GlobalNoArgumentEvent update
        {
            add { onUpdate += value; }
            remove { onUpdate -= value; }
        }

        [Documentation("Copies of the effects the game has created")]
        public FfbEffect[] effects
        {
            get { return source.State.Effects.Select(e => e.Clone()).ToArray(); }
        }

        [Documentation("Overall strength set by the game, 0 to 1")]
        public double gain
        {
            get { return source.State.Gain; }
        }

        [Documentation("False when the game has disabled the actuators")]
        public bool actuatorsEnabled
        {
            get { return source.State.ActuatorsEnabled; }
        }

        [Documentation("True when the game has paused all effects")]
        public bool paused
        {
            get { return source.State.Paused; }
        }
    }
}
