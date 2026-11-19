using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Plugins.Ffb
{
    public class FfbState : IFfbSink
    {
        private readonly SortedDictionary<int, FfbEffect> effects;
        private readonly Dictionary<int, int> loopCounts;

        public FfbState()
        {
            effects = new SortedDictionary<int, FfbEffect>();
            loopCounts = new Dictionary<int, int>();
            Gain = 1;
            ActuatorsEnabled = true;
        }

        public double Gain { get; private set; }
        public bool ActuatorsEnabled { get; private set; }
        public bool Paused { get; private set; }

        public IEnumerable<FfbEffect> Effects
        {
            get { return effects.Values; }
        }

        public FfbEffect GetEffectCopy(int index)
        {
            FfbEffect effect;
            return effects.TryGetValue(index, out effect) ? effect.Clone() : new FfbEffect();
        }

        public void SetEffect(int index, FfbEffect effect)
        {
            FfbEffect existing;
            var playing = effects.TryGetValue(index, out existing) && existing.playing;

            var copy = effect.Clone();
            copy.playing = playing;
            effects[index] = copy;
        }

        public void StartEffect(int index, int loopCount, bool solo)
        {
            if (solo)
                foreach (var effect in effects.Values)
                    effect.playing = false;

            FfbEffect started;
            if (!effects.TryGetValue(index, out started))
                return;

            started.playing = true;
            loopCounts[index] = loopCount;
        }

        public void StopEffect(int index)
        {
            FfbEffect effect;
            if (effects.TryGetValue(index, out effect))
                effect.playing = false;
        }

        public void RemoveEffect(int index)
        {
            effects.Remove(index);
            loopCounts.Remove(index);
        }

        public void SetGain(double gain)
        {
            Gain = gain;
        }

        public void Control(FfbControl control)
        {
            switch (control)
            {
                case FfbControl.EnableActuators:
                    ActuatorsEnabled = true;
                    break;
                case FfbControl.DisableActuators:
                    ActuatorsEnabled = false;
                    break;
                case FfbControl.StopAll:
                    foreach (var effect in effects.Values)
                        effect.playing = false;
                    break;
                case FfbControl.Reset:
                    effects.Clear();
                    loopCounts.Clear();
                    Paused = false;
                    ActuatorsEnabled = true;
                    break;
                case FfbControl.Pause:
                    Paused = true;
                    break;
                case FfbControl.Continue:
                    Paused = false;
                    break;
            }
        }

        // Brings a sink that is attached late up to date with what the source has already sent
        public void ReplayTo(IFfbSink sink)
        {
            sink.SetGain(Gain);

            foreach (var pair in effects)
                sink.SetEffect(pair.Key, pair.Value);

            foreach (var pair in effects.Where(p => p.Value.playing))
            {
                int loopCount;
                sink.StartEffect(pair.Key, loopCounts.TryGetValue(pair.Key, out loopCount) ? loopCount : 1, false);
            }

            if (Paused)
                sink.Control(FfbControl.Pause);
            if (!ActuatorsEnabled)
                sink.Control(FfbControl.DisableActuators);
        }
    }
}
