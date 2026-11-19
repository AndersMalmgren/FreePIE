using System.Collections.Generic;
using FreePIE.Core.Plugins.Ffb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core.Plugins.Ffb
{
    [TestClass]
    public class FfbSinkGlobalTests
    {
        private FakeDevice device;
        private FfbSinkGlobal ffb;
        private RecordingFfbSink scriptChannel;

        [TestInitialize]
        public void Context()
        {
            device = new FakeDevice();
            ffb = new FfbSinkGlobal(device);
            scriptChannel = device.Channels[0];
        }

        [TestMethod]
        public void Script_effects_are_uploaded_and_started_when_flushed()
        {
            ffb.setConstant(0.5);

            Assert.AreEqual(0, scriptChannel.Calls.Count);
            ffb.Flush();
            CollectionAssert.AreEqual(new[] { "set -1 Constant", "start -1 1" }, scriptChannel.Calls);
        }

        [TestMethod]
        public void Unchanged_effects_are_not_uploaded_again()
        {
            ffb.setConstant(0.5);
            ffb.Flush();
            scriptChannel.Calls.Clear();

            ffb.setConstant(0.5);
            ffb.Flush();

            Assert.AreEqual(0, scriptChannel.Calls.Count);
        }

        [TestMethod]
        public void Changed_effects_are_updated_without_restarting()
        {
            ffb.setConstant(0.5);
            ffb.Flush();
            scriptChannel.Calls.Clear();

            ffb.setConstant(-0.5);
            ffb.Flush();

            CollectionAssert.AreEqual(new[] { "set -1 Constant" }, scriptChannel.Calls);
        }

        [TestMethod]
        public void Effect_slots_start_and_stop_with_playing()
        {
            var effect = ffb.effects[0];
            effect.type = FfbEffectType.Sine;
            effect.playing = true;
            ffb.Flush();
            ffb.stopAll();
            ffb.Flush();

            CollectionAssert.AreEqual(new[] { "set 0 Sine", "start 0 1", "stop 0" }, scriptChannel.Calls);
        }

        [TestMethod]
        public void Bridging_a_source_creates_a_channel_once()
        {
            var source = new FfbSource();

            ffb.bridge(source);
            ffb.bridge(source);

            Assert.AreEqual(2, device.Channels.Count);
        }

        [TestMethod]
        public void Bridged_source_commands_reach_the_channel()
        {
            var source = new FfbSource();
            ffb.bridge(source);

            source.UpdateEffect(1, e => e.type = FfbEffectType.Damper);
            source.Pump();

            CollectionAssert.Contains(device.Channels[1].Calls, "set 1 Damper");
        }

        [TestMethod]
        public void Unbridging_resets_and_releases_the_channel()
        {
            var source = new FfbSource();
            ffb.bridge(source);
            var channel = device.Channels[1];

            ffb.unbridge(source);

            CollectionAssert.Contains(channel.Calls, "control Reset");
            CollectionAssert.Contains(device.Released, channel);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Periodic_requires_a_periodic_type()
        {
            ffb.setPeriodic(FfbEffectType.Spring, 1, 100);
        }

        private class FakeDevice : IFfbSinkDevice
        {
            public FakeDevice()
            {
                Channels = new List<RecordingFfbSink>();
                Released = new List<IFfbSink>();
            }

            public List<RecordingFfbSink> Channels { get; private set; }
            public List<IFfbSink> Released { get; private set; }

            public IFfbSink CreateChannel()
            {
                var channel = new RecordingFfbSink();
                Channels.Add(channel);
                return channel;
            }

            public void ReleaseChannel(IFfbSink channel)
            {
                Released.Add(channel);
            }

            public double Gain { get; set; }
        }
    }
}
