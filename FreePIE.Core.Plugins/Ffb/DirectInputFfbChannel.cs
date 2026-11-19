using System;
using System.Collections.Generic;
using System.Linq;
using SlimDX.DirectInput;

namespace FreePIE.Core.Plugins.Ffb
{
    public class DirectInputFfbChannel : IFfbSink, IDisposable
    {
        // The axes of an effect are fixed once it is created, DirectInput fails with ERROR_ALREADY_INITIALIZED if they are passed again
        private const EffectParameterFlags UpdatableParameters = EffectParameterFlags.All & ~EffectParameterFlags.Axes;

        private readonly DirectInputFfbDevice device;
        private readonly Dictionary<int, Effect> effects;
        private readonly Dictionary<int, FfbEffectType> types;
        private readonly Dictionary<int, int> playing;
        private bool paused;

        public DirectInputFfbChannel(DirectInputFfbDevice device)
        {
            this.device = device;
            effects = new Dictionary<int, Effect>();
            types = new Dictionary<int, FfbEffectType>();
            playing = new Dictionary<int, int>();
        }

        public void SetEffect(int index, FfbEffect effect)
        {
            var guid = DirectInputFfbMapper.ToGuid(effect.type);
            if (guid == null || !device.Supports(guid.Value))
            {
                RemoveEffect(index);
                return;
            }

            var parameters = DirectInputFfbMapper.ToParameters(effect, device.Axes);

            Effect existing;
            if (effects.TryGetValue(index, out existing) && types[index] == effect.type)
            {
                existing.SetParameters(parameters, UpdatableParameters);
                return;
            }

            var wasPlaying = playing.ContainsKey(index);
            var loopCount = wasPlaying ? playing[index] : 1;
            RemoveEffect(index);

            effects[index] = new Effect(device.Joystick, guid.Value, parameters);
            types[index] = effect.type;

            if (wasPlaying)
                StartEffect(index, loopCount, false);
        }

        public void StartEffect(int index, int loopCount, bool solo)
        {
            Effect effect;
            if (!effects.TryGetValue(index, out effect))
                return;

            // DirectInput solo would also stop effects of other channels on the device
            if (solo)
                foreach (var other in playing.Keys.Where(i => i != index).ToList())
                    StopEffect(other);

            playing[index] = loopCount;

            if (!paused)
                effect.Start(loopCount);
        }

        public void StopEffect(int index)
        {
            playing.Remove(index);

            Effect effect;
            if (effects.TryGetValue(index, out effect))
                effect.Stop();
        }

        public void RemoveEffect(int index)
        {
            Effect effect;
            if (!effects.TryGetValue(index, out effect))
                return;

            effect.Stop();
            effect.Dispose();
            effects.Remove(index);
            types.Remove(index);
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
                    playing.Keys.ToList().ForEach(StopEffect);
                    break;
                case FfbControl.Reset:
                    effects.Keys.ToList().ForEach(RemoveEffect);
                    paused = false;
                    device.ActuatorsEnabled = true;
                    break;
                case FfbControl.Pause:
                    paused = true;
                    foreach (var index in playing.Keys)
                        effects[index].Stop();
                    break;
                case FfbControl.Continue:
                    paused = false;
                    foreach (var pair in playing)
                        effects[pair.Key].Start(pair.Value);
                    break;
            }
        }

        public void Dispose()
        {
            Control(FfbControl.Reset);
        }
    }
}
