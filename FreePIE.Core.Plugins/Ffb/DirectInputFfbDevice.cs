using System;
using System.Collections.Generic;
using System.Linq;
using SlimDX.DirectInput;

namespace FreePIE.Core.Plugins.Ffb
{
    public class DirectInputFfbDevice : IFfbSinkDevice, IDisposable
    {
        private readonly Joystick joystick;
        private readonly HashSet<Guid> supportedEffects;
        private readonly List<DirectInputFfbChannel> channels;
        private double gain;
        private bool actuatorsEnabled;

        public DirectInputFfbDevice(Joystick joystick)
        {
            this.joystick = joystick;

            if ((joystick.Capabilities.Flags & DeviceFlags.ForceFeedback) == 0)
                throw new Exception(string.Format("{0} does not support force feedback", joystick.Information.InstanceName));

            Axes = joystick.GetObjects()
                .Where(o => (o.ObjectType & ObjectDeviceType.ForceFeedbackActuator) != 0)
                .Take(2)
                .Select(o => (int) o.ObjectType)
                .ToArray();

            if (Axes.Length == 0)
                throw new Exception(string.Format("{0} has no force feedback axes", joystick.Information.InstanceName));

            supportedEffects = new HashSet<Guid>(joystick.GetEffects().Select(e => e.Guid));
            channels = new List<DirectInputFfbChannel>();
            gain = 1;
            actuatorsEnabled = true;

            // Auto center fights every effect and can only be changed while the device is unacquired
            joystick.Unacquire();
            joystick.Properties.AutoCenter = false;
            joystick.Acquire();
            ApplyGain();
        }

        public Joystick Joystick
        {
            get { return joystick; }
        }

        public int[] Axes { get; private set; }

        public bool Supports(Guid effect)
        {
            return supportedEffects.Contains(effect);
        }

        public IFfbSink CreateChannel()
        {
            var channel = new DirectInputFfbChannel(this);
            channels.Add(channel);

            return channel;
        }

        public void ReleaseChannel(IFfbSink channel)
        {
            var directInputChannel = (DirectInputFfbChannel) channel;
            if (channels.Remove(directInputChannel))
                directInputChannel.Dispose();
        }

        public double Gain
        {
            get { return gain; }
            set
            {
                gain = value;
                ApplyGain();
            }
        }

        public bool ActuatorsEnabled
        {
            get { return actuatorsEnabled; }
            set
            {
                actuatorsEnabled = value;
                ApplyGain();
            }
        }

        // SlimDX does not expose SendForceFeedbackCommand, disabled actuators are emulated with zero gain
        private void ApplyGain()
        {
            joystick.Properties.ForceFeedbackGain = actuatorsEnabled ? DirectInputFfbMapper.ToUnsigned(gain) : 0;
        }

        public void Dispose()
        {
            channels.ForEach(c => c.Dispose());
            channels.Clear();
        }
    }
}
