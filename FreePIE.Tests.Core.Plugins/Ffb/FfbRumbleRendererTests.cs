using FreePIE.Core.Plugins.Ffb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core.Plugins.Ffb
{
    [TestClass]
    public class FfbRumbleRendererTests
    {
        private const double Delta = 0.0001;

        private static void Render(FfbEffect effect, double elapsed, out double large, out double small, int loopCount = 1)
        {
            FfbRumbleRenderer.Render(effect, elapsed, loopCount, out large, out small);
        }

        [TestMethod]
        public void Rumble_is_passed_through_scaled_by_gain()
        {
            double large, small;
            Render(new FfbEffect { type = FfbEffectType.Rumble, largeMotor = 0.8, smallMotor = 0.4, gain = 0.5 }, 0, out large, out small);

            Assert.AreEqual(0.4, large, Delta);
            Assert.AreEqual(0.2, small, Delta);
        }

        [TestMethod]
        public void Constant_force_drives_the_large_motor_with_its_strength()
        {
            double large, small;
            Render(new FfbEffect { type = FfbEffectType.Constant, magnitude = -0.6 }, 0, out large, out small);

            Assert.AreEqual(0.6, large, Delta);
            Assert.AreEqual(0, small, Delta);
        }

        [TestMethod]
        public void Fast_periodic_effects_drive_the_small_motor()
        {
            double large, small;
            Render(new FfbEffect { type = FfbEffectType.Sine, magnitude = 0.7, period = 20 }, 0, out large, out small);

            Assert.AreEqual(0, large, Delta);
            Assert.AreEqual(0.7, small, Delta);
        }

        [TestMethod]
        public void Slow_periodic_effects_follow_the_wave()
        {
            var effect = new FfbEffect { type = FfbEffectType.Sine, magnitude = 1, period = 400 };
            double large, small;

            Render(effect, 100, out large, out small);
            Assert.AreEqual(1, large, Delta);

            Render(effect, 200, out large, out small);
            Assert.AreEqual(0, large, Delta);
        }

        [TestMethod]
        public void Nothing_is_rendered_before_the_start_delay_or_after_the_duration()
        {
            var effect = new FfbEffect { type = FfbEffectType.Constant, magnitude = 1, startDelay = 100, duration = 200 };
            double large, small;

            Render(effect, 50, out large, out small);
            Assert.AreEqual(0, large, Delta);

            Render(effect, 150, out large, out small);
            Assert.AreEqual(1, large, Delta);

            Render(effect, 350, out large, out small);
            Assert.AreEqual(0, large, Delta);
        }

        [TestMethod]
        public void Effects_repeat_for_their_loop_count()
        {
            var effect = new FfbEffect { type = FfbEffectType.Constant, magnitude = 1, duration = 100 };
            double large, small;

            Render(effect, 150, out large, out small, 2);
            Assert.AreEqual(1, large, Delta);

            Render(effect, 1000, out large, out small, FfbEffect.Infinite);
            Assert.AreEqual(1, large, Delta);
        }

        [TestMethod]
        public void Ramp_changes_over_the_duration()
        {
            double large, small;
            Render(new FfbEffect { type = FfbEffectType.Ramp, rampStart = 0, rampEnd = 1, duration = 100 }, 25, out large, out small);

            Assert.AreEqual(0.25, large, Delta);
        }

        [TestMethod]
        public void Envelope_attack_and_fade_are_applied()
        {
            var effect = new FfbEffect { type = FfbEffectType.Constant, magnitude = 1, duration = 1000, attackLevel = 0, attackTime = 100, fadeLevel = 0, fadeTime = 100 };
            double large, small;

            Render(effect, 50, out large, out small);
            Assert.AreEqual(0.5, large, Delta);

            Render(effect, 500, out large, out small);
            Assert.AreEqual(1, large, Delta);

            Render(effect, 950, out large, out small);
            Assert.AreEqual(0.5, large, Delta);
        }

        [TestMethod]
        public void Conditions_are_not_rendered()
        {
            var effect = new FfbEffect { type = FfbEffectType.Spring };
            effect.conditionX.positiveCoefficient = 1;
            double large, small;

            Render(effect, 0, out large, out small);

            Assert.AreEqual(0, large, Delta);
            Assert.AreEqual(0, small, Delta);
        }
    }
}
