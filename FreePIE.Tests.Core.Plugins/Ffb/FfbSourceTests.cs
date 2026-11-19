using System.Linq;
using FreePIE.Core.Plugins.Ffb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core.Plugins.Ffb
{
    [TestClass]
    public class FfbSourceTests
    {
        private FfbSource source;
        private RecordingFfbSink sink;

        [TestInitialize]
        public void Context()
        {
            source = new FfbSource();
            sink = new RecordingFfbSink();
            source.AddSink(sink);
            sink.Calls.Clear();
        }

        [TestMethod]
        public void Commands_are_not_forwarded_until_the_source_is_pumped()
        {
            source.UpdateEffect(1, e => e.type = FfbEffectType.Constant);

            Assert.AreEqual(0, sink.Calls.Count);
            Assert.IsTrue(source.Pump());
            CollectionAssert.AreEqual(new[] { "set 1 Constant" }, sink.Calls);
        }

        [TestMethod]
        public void Pump_returns_false_when_nothing_was_queued()
        {
            Assert.IsFalse(source.Pump());
        }

        [TestMethod]
        public void Partial_updates_keep_the_parameters_from_earlier_packets()
        {
            source.UpdateEffect(1, e => e.type = FfbEffectType.Sine);
            source.UpdateEffect(1, e => e.period = 250);
            source.Pump();

            Assert.AreEqual(FfbEffectType.Sine, sink.Effects[1].type);
            Assert.AreEqual(250, sink.Effects[1].period);
        }

        [TestMethod]
        public void State_tracks_which_effects_are_playing()
        {
            source.UpdateEffect(1, e => e.type = FfbEffectType.Constant);
            source.UpdateEffect(2, e => e.type = FfbEffectType.Sine);
            source.StartEffect(1, 1, false);
            source.StartEffect(2, 1, true);
            source.Pump();

            var playing = source.State.Effects.Where(e => e.playing).Select(e => e.type).ToArray();
            CollectionAssert.AreEqual(new[] { FfbEffectType.Sine }, playing);
        }

        [TestMethod]
        public void A_sink_added_late_gets_the_current_state_replayed()
        {
            source.SetGain(0.5);
            source.UpdateEffect(3, e => e.type = FfbEffectType.Spring);
            source.StartEffect(3, -1, false);
            source.Control(FfbControl.Pause);
            source.Pump();

            var lateSink = new RecordingFfbSink();
            source.AddSink(lateSink);

            CollectionAssert.AreEqual(new[] { "gain 0.5", "set 3 Spring", "start 3 -1", "control Pause" }, lateSink.Calls);
        }

        [TestMethod]
        public void Adding_the_same_sink_twice_does_not_replay_again()
        {
            source.AddSink(sink);

            Assert.AreEqual(0, sink.Calls.Count);
        }

        [TestMethod]
        public void A_removed_sink_is_reset_and_gets_no_more_commands()
        {
            source.RemoveSink(sink);
            source.UpdateEffect(1, e => e.type = FfbEffectType.Constant);
            source.Pump();

            CollectionAssert.AreEqual(new[] { "control Reset" }, sink.Calls);
        }

        [TestMethod]
        public void Reset_clears_the_effects_of_the_state()
        {
            source.UpdateEffect(1, e => e.type = FfbEffectType.Constant);
            source.Control(FfbControl.Reset);
            source.Pump();

            Assert.AreEqual(0, source.State.Effects.Count());
        }

        [TestMethod]
        public void Errors_from_the_driver_thread_are_thrown_when_pumped()
        {
            source.ReportError(new System.InvalidOperationException("bad packet"));

            try
            {
                source.Pump();
                Assert.Fail("Expected the error to be thrown");
            }
            catch (System.Exception e)
            {
                StringAssert.Contains(e.Message, "bad packet");
            }
        }
    }
}
