using FreePIE.Core.Plugins.Ffb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vJoyInterfaceWrap;

namespace FreePIE.Tests.Core.Plugins.Ffb
{
    [TestClass]
    public class VJoyFfbConverterTests
    {
        private const double Delta = 0.0001;

        [TestMethod]
        public void Effect_report_is_converted_to_normalized_values()
        {
            var effect = new FfbEffect();
            var report = new vJoy.FFB_EFF_REPORT { EffectType = FFBEType.ET_SINE, Duration = 500, Gain = 255, Polar = true, Direction = 64 };

            VJoyFfbConverter.Apply(effect, report);

            Assert.AreEqual(FfbEffectType.Sine, effect.type);
            Assert.AreEqual(500, effect.duration);
            Assert.AreEqual(1, effect.gain, Delta);
            Assert.AreEqual(64 * 360 / 255.0, effect.direction, Delta);
        }

        [TestMethod]
        public void Max_duration_means_infinite()
        {
            var effect = new FfbEffect();

            VJoyFfbConverter.Apply(effect, new vJoy.FFB_EFF_REPORT { Duration = 0xFFFF });

            Assert.AreEqual(FfbEffect.Infinite, effect.duration);
        }

        [TestMethod]
        public void Constant_magnitude_is_scaled_to_minus_one_to_one()
        {
            var effect = new FfbEffect();

            VJoyFfbConverter.Apply(effect, new vJoy.FFB_EFF_CONSTANT { Magnitude = -5000 });

            Assert.AreEqual(-0.5, effect.magnitude, Delta);
        }

        [TestMethod]
        public void Periodic_parameters_are_converted()
        {
            var effect = new FfbEffect();

            VJoyFfbConverter.Apply(effect, new vJoy.FFB_EFF_PERIOD { Magnitude = 10000, Offset = -2500, Phase = 9000, Period = 40 });

            Assert.AreEqual(1, effect.magnitude, Delta);
            Assert.AreEqual(-0.25, effect.offset, Delta);
            Assert.AreEqual(90, effect.phase, Delta);
            Assert.AreEqual(40, effect.period);
        }

        [TestMethod]
        public void Condition_goes_to_the_axis_it_is_reported_for()
        {
            var effect = new FfbEffect();

            VJoyFfbConverter.Apply(effect, new vJoy.FFB_EFF_COND { isY = true, PosCoeff = 10000, NegCoeff = -10000, PosSatur = 5000 });

            Assert.AreEqual(1, effect.conditionY.positiveCoefficient, Delta);
            Assert.AreEqual(-1, effect.conditionY.negativeCoefficient, Delta);
            Assert.AreEqual(0.5, effect.conditionY.positiveSaturation, Delta);
            Assert.AreEqual(0, effect.conditionX.positiveCoefficient, Delta);
        }

        [TestMethod]
        public void Loop_count_255_means_infinite()
        {
            Assert.AreEqual(FfbEffect.Infinite, VJoyFfbConverter.ToLoopCount(255));
            Assert.AreEqual(3, VJoyFfbConverter.ToLoopCount(3));
        }

        [TestMethod]
        public void Device_controls_are_mapped()
        {
            Assert.AreEqual(FfbControl.StopAll, VJoyFfbConverter.ToControl(FFB_CTRL.CTRL_STOPALL));
            Assert.AreEqual(FfbControl.Reset, VJoyFfbConverter.ToControl(FFB_CTRL.CTRL_DEVRST));
            Assert.AreEqual(FfbControl.Pause, VJoyFfbConverter.ToControl(FFB_CTRL.CTRL_DEVPAUSE));
        }
    }
}
