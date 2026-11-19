using System.Collections.Generic;
using FreePIE.Core.Plugins.Ffb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core.Plugins.Ffb
{
    [TestClass]
    public class RumbleFfbDeviceTests
    {
        private const double Delta = 0.0001;

        private double now;
        private List<double[]> motors;
        private RumbleFfbDevice device;
        private IFfbSink channel;

        [TestInitialize]
        public void Context()
        {
            now = 0;
            motors = new List<double[]>();
            device = new RumbleFfbDevice((large, small) => motors.Add(new[] { large, small }), () => now);
            channel = device.CreateChannel();
        }

        private double[] LastMotors
        {
            get { return motors[motors.Count - 1]; }
        }

        [TestMethod]
        public void Playing_effects_set_the_motors_when_rendered()
        {
            channel.SetEffect(1, new FfbEffect { type = FfbEffectType.Rumble, largeMotor = 0.5, smallMotor = 0.25 });
            channel.StartEffect(1, 1, false);

            device.Render();

            Assert.AreEqual(0.5, LastMotors[0], Delta);
            Assert.AreEqual(0.25, LastMotors[1], Delta);
        }

        [TestMethod]
        public void Motors_are_only_set_when_they_change()
        {
            channel.SetEffect(1, new FfbEffect { type = FfbEffectType.Rumble, largeMotor = 0.5 });
            channel.StartEffect(1, 1, false);

            device.Render();
            device.Render();

            Assert.AreEqual(1, motors.Count);
        }

        [TestMethod]
        public void Channels_are_added_together_and_limited()
        {
            var other = device.CreateChannel();
            channel.SetEffect(1, new FfbEffect { type = FfbEffectType.Rumble, largeMotor = 0.7 });
            channel.StartEffect(1, 1, false);
            other.SetEffect(1, new FfbEffect { type = FfbEffectType.Rumble, largeMotor = 0.7 });
            other.StartEffect(1, 1, false);

            device.Render();

            Assert.AreEqual(1, LastMotors[0], Delta);
        }

        [TestMethod]
        public void Effects_end_after_their_duration()
        {
            channel.SetEffect(1, new FfbEffect { type = FfbEffectType.Constant, magnitude = 1, duration = 100 });
            channel.StartEffect(1, 1, false);
            device.Render();

            now = 150;
            device.Render();

            Assert.AreEqual(0, LastMotors[0], Delta);
        }

        [TestMethod]
        public void Gain_and_disabled_actuators_are_applied()
        {
            channel.SetEffect(1, new FfbEffect { type = FfbEffectType.Rumble, largeMotor = 1 });
            channel.StartEffect(1, 1, false);

            channel.SetGain(0.5);
            device.Render();
            Assert.AreEqual(0.5, LastMotors[0], Delta);

            channel.Control(FfbControl.DisableActuators);
            device.Render();
            Assert.AreEqual(0, LastMotors[0], Delta);
        }

        [TestMethod]
        public void Paused_channels_are_silent_until_continued()
        {
            channel.SetEffect(1, new FfbEffect { type = FfbEffectType.Rumble, largeMotor = 1 });
            channel.StartEffect(1, 1, false);
            device.Render();

            channel.Control(FfbControl.Pause);
            device.Render();
            Assert.AreEqual(0, LastMotors[0], Delta);

            channel.Control(FfbControl.Continue);
            device.Render();
            Assert.AreEqual(1, LastMotors[0], Delta);
        }

        [TestMethod]
        public void Disposing_stops_the_motors()
        {
            channel.SetEffect(1, new FfbEffect { type = FfbEffectType.Rumble, largeMotor = 1 });
            channel.StartEffect(1, 1, false);
            device.Render();

            device.Dispose();

            Assert.AreEqual(0, LastMotors[0], Delta);
        }
    }
}
