using System;
using System.Linq;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Ffb
{
    [Documentation("Rumble of an Xbox 360 controller. Bridge to it, or set effects, to rumble the physical controller. Bridge from it, or read largeMotor and smallMotor, to get the rumble games send to the emulated controller")]
    public class XBox360FfbGlobal : FfbSinkGlobal, IFfbSource
    {
        private readonly Func<FfbSource> createSource;
        private FfbSource source;
        private event GlobalNoArgumentEvent onUpdate;

        public XBox360FfbGlobal(IFfbSinkDevice device, Func<FfbSource> createSource) : base(device)
        {
            this.createSource = createSource;
        }

        // Creating the source connects the emulated controller, so it is only done when the rumble from games is used
        private FfbSource Source
        {
            get
            {
                EnsureSource();
                return source;
            }
        }

        private void EnsureSource()
        {
            if (source == null)
                source = createSource();
        }

        internal void Pump()
        {
            if (source != null && source.Pump() && onUpdate != null)
                onUpdate();
        }

        void IFfbSource.AddSink(IFfbSink sink)
        {
            Source.AddSink(sink);
        }

        void IFfbSource.RemoveSink(IFfbSink sink)
        {
            Source.RemoveSink(sink);
        }

        [Documentation("Raised when a game has changed the rumble of the emulated controller, assign a function with +=")]
        public event GlobalNoArgumentEvent update
        {
            add
            {
                EnsureSource();
                onUpdate += value;
            }
            remove { onUpdate -= value; }
        }

        [Documentation("Speed a game has set for the large motor of the emulated controller, 0 to 1")]
        public double largeMotor
        {
            get { return GameRumble.largeMotor; }
        }

        [Documentation("Speed a game has set for the small motor of the emulated controller, 0 to 1")]
        public double smallMotor
        {
            get { return GameRumble.smallMotor; }
        }

        private FfbEffect GameRumble
        {
            get { return Source.State.Effects.FirstOrDefault(e => e.playing) ?? new FfbEffect(); }
        }
    }
}
