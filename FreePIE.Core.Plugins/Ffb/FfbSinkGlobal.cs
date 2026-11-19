using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;

namespace FreePIE.Core.Plugins.Ffb
{
    [Documentation("Force feedback of a device, bridge force feedback from a game to it or play your own effects")]
    public class FfbSinkGlobal
    {
        private const int ConstantSlot = -1;
        private const int PeriodicSlot = -2;
        private const int RumbleSlot = -3;
        private const int ConditionSlotBase = -100;

        private readonly IFfbSinkDevice device;
        private readonly IFfbSink scriptChannel;
        private readonly Dictionary<IFfbSource, IFfbSink> bridges;
        private readonly Dictionary<int, FfbEffect> slots;
        private readonly Dictionary<int, FfbEffect> uploaded;

        public FfbSinkGlobal(IFfbSinkDevice device)
        {
            this.device = device;
            scriptChannel = device.CreateChannel();
            bridges = new Dictionary<IFfbSource, IFfbSink>();
            slots = new Dictionary<int, FfbEffect>();
            uploaded = new Dictionary<int, FfbEffect>();
            effects = new GlobalIndexer<FfbEffect>(GetSlot);
        }

        // Effects changed by the script are uploaded once per iteration, so scripts can set them every iteration
        internal void Flush()
        {
            foreach (var pair in slots)
            {
                var index = pair.Key;
                var effect = pair.Value;

                FfbEffect previous;
                var known = uploaded.TryGetValue(index, out previous);

                if (!known || !effect.SameParametersAs(previous))
                    scriptChannel.SetEffect(index, effect);

                var wasPlaying = known && previous.playing;
                if (effect.playing && !wasPlaying)
                    scriptChannel.StartEffect(index, 1, false);
                else if (!effect.playing && wasPlaying)
                    scriptChannel.StopEffect(index);

                uploaded[index] = effect.Clone();
            }
        }

        private FfbEffect GetSlot(int index)
        {
            FfbEffect effect;
            if (!slots.TryGetValue(index, out effect))
                slots[index] = effect = new FfbEffect();

            return effect;
        }

        [Documentation("Plays the force feedback of a source, for example vJoy[0].ffb, on this device")]
        public void bridge(IFfbSource source)
        {
            if (bridges.ContainsKey(source))
                return;

            var channel = device.CreateChannel();
            bridges[source] = channel;
            source.AddSink(channel);
        }

        [Documentation("Stops playing the force feedback of a source that was bridged")]
        public void unbridge(IFfbSource source)
        {
            IFfbSink channel;
            if (!bridges.TryGetValue(source, out channel))
                return;

            bridges.Remove(source);
            source.RemoveSink(channel);
            device.ReleaseChannel(channel);
        }

        [Documentation("Effects played by the script, index by any number. Set type and parameters, then set playing to True")]
        public GlobalIndexer<FfbEffect> effects { get; private set; }

        [Documentation("Overall strength of the force feedback, 0 to 1")]
        public double gain
        {
            get { return device.Gain; }
            set { device.Gain = value; }
        }

        [Documentation("Plays a constant force, -1 to 1")]
        public void setConstant(double magnitude)
        {
            setConstant(magnitude, 90);
        }

        [Documentation("Plays a constant force, -1 to 1, coming from direction in degrees")]
        public void setConstant(double magnitude, double direction)
        {
            var effect = GetSlot(ConstantSlot);
            effect.type = FfbEffectType.Constant;
            effect.magnitude = magnitude;
            effect.direction = direction;
            effect.playing = true;
        }

        [Documentation("Plays a periodic effect, magnitude 0 to 1 and period in milliseconds")]
        public void setPeriodic(FfbEffectType type, double magnitude, int period)
        {
            setPeriodic(type, magnitude, period, 90);
        }

        [Documentation("Plays a periodic effect, magnitude 0 to 1 and period in milliseconds, coming from direction in degrees")]
        public void setPeriodic(FfbEffectType type, double magnitude, int period, double direction)
        {
            if (!type.IsPeriodic())
                throw new ArgumentException(string.Format("{0} is not a periodic effect", type));

            var effect = GetSlot(PeriodicSlot);
            effect.type = type;
            effect.magnitude = magnitude;
            effect.period = period;
            effect.direction = direction;
            effect.playing = true;
        }

        [Documentation("Plays a spring, damper, inertia or friction condition on all axes, coefficient -1 to 1")]
        public void setCondition(FfbEffectType type, double coefficient)
        {
            if (!type.IsCondition())
                throw new ArgumentException(string.Format("{0} is not a condition effect", type));

            var effect = GetSlot(ConditionSlotBase - (int) type);
            effect.type = type;
            foreach (var condition in new[] { effect.conditionX, effect.conditionY })
            {
                condition.positiveCoefficient = coefficient;
                condition.negativeCoefficient = coefficient;
            }
            effect.playing = true;
        }

        [Documentation("Rumbles the large and small motor, 0 to 1. Devices without rumble motors vibrate instead")]
        public void setRumble(double largeMotor, double smallMotor)
        {
            var effect = GetSlot(RumbleSlot);
            effect.type = FfbEffectType.Rumble;
            effect.largeMotor = largeMotor;
            effect.smallMotor = smallMotor;
            effect.playing = true;
        }

        [Documentation("Stops all effects played by the script")]
        public void stopAll()
        {
            foreach (var effect in slots.Values)
                effect.playing = false;
        }
    }
}
