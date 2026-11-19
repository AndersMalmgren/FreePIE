using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Plugins.Ffb
{
    public class RumbleFfbChannel : IFfbSink
    {
        private readonly RumbleFfbDevice device;
        private readonly Dictionary<int, FfbEffect> effects;
        private readonly Dictionary<int, Playback> playing;
        private bool paused;

        public RumbleFfbChannel(RumbleFfbDevice device)
        {
            this.device = device;
            effects = new Dictionary<int, FfbEffect>();
            playing = new Dictionary<int, Playback>();
        }

        public void Render(double now, ref double large, ref double small)
        {
            if (paused)
                return;

            foreach (var pair in playing)
            {
                FfbEffect effect;
                if (!effects.TryGetValue(pair.Key, out effect))
                    continue;

                double effectLarge, effectSmall;
                FfbRumbleRenderer.Render(effect, now - pair.Value.Start, pair.Value.LoopCount, out effectLarge, out effectSmall);
                large += effectLarge;
                small += effectSmall;
            }
        }

        public void SetEffect(int index, FfbEffect effect)
        {
            effects[index] = effect.Clone();
        }

        public void StartEffect(int index, int loopCount, bool solo)
        {
            if (solo)
                playing.Clear();

            playing[index] = new Playback(device.Now, loopCount);
        }

        public void StopEffect(int index)
        {
            playing.Remove(index);
        }

        public void RemoveEffect(int index)
        {
            effects.Remove(index);
            playing.Remove(index);
        }

        public void SetGain(double gain)
        {
            device.Gain = gain;
        }

        public void Control(FfbControl control)
        {
            switch (control)
            {
                case FfbControl.EnableActuators:
                    device.ActuatorsEnabled = true;
                    break;
                case FfbControl.DisableActuators:
                    device.ActuatorsEnabled = false;
                    break;
                case FfbControl.StopAll:
                    playing.Clear();
                    break;
                case FfbControl.Reset:
                    effects.Clear();
                    playing.Clear();
                    paused = false;
                    device.ActuatorsEnabled = true;
                    break;
                case FfbControl.Pause:
                    paused = true;
                    break;
                case FfbControl.Continue:
                    paused = false;
                    var now = device.Now;
                    foreach (var index in playing.Keys.ToList())
                        playing[index] = new Playback(now, playing[index].LoopCount);
                    break;
            }
        }

        private class Playback
        {
            public Playback(double start, int loopCount)
            {
                Start = start;
                LoopCount = loopCount;
            }

            public double Start { get; private set; }
            public int LoopCount { get; private set; }
        }
    }
}
