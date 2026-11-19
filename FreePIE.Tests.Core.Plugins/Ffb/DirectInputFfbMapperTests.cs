using FreePIE.Core.Plugins.Ffb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimDX.DirectInput;

namespace FreePIE.Tests.Core.Plugins.Ffb
{
    [TestClass]
    public class DirectInputFfbMapperTests
    {
        private static readonly int[] OneAxis = { 1 };
        private static readonly int[] TwoAxes = { 1, 2 };

        [TestMethod]
        public void Two_axis_effects_use_polar_direction_in_hundredths_of_degrees()
        {
            var effect = new FfbEffect { type = FfbEffectType.Constant, magnitude = 0.5, direction = 270 };

            var parameters = DirectInputFfbMapper.ToParameters(effect, TwoAxes);

            int[] axes, directions;
            parameters.GetAxes(out axes, out directions);
            Assert.IsTrue((parameters.Flags & EffectFlags.Polar) != 0);
            Assert.AreEqual(27000, directions[0]);
            Assert.AreEqual(5000, parameters.Parameters.AsConstantForce().Magnitude);
        }

        [TestMethod]
        public void One_axis_effects_fold_the_direction_into_the_force()
        {
            var effect = new FfbEffect { type = FfbEffectType.Constant, magnitude = 0.5, direction = 270 };

            var parameters = DirectInputFfbMapper.ToParameters(effect, OneAxis);

            Assert.AreEqual(-5000, parameters.Parameters.AsConstantForce().Magnitude);
        }

        [TestMethod]
        public void One_axis_periodic_effects_from_the_opposite_side_are_phase_shifted()
        {
            var effect = new FfbEffect { type = FfbEffectType.Sine, magnitude = 1, period = 50, phase = 0, direction = 270 };

            var periodic = DirectInputFfbMapper.ToParameters(effect, OneAxis).Parameters.AsPeriodicForce();

            Assert.AreEqual(10000, periodic.Magnitude);
            Assert.AreEqual(18000, periodic.Phase);
            Assert.AreEqual(50000, periodic.Period);
        }

        [TestMethod]
        public void Conditions_are_created_for_each_axis()
        {
            var effect = new FfbEffect { type = FfbEffectType.Spring };
            effect.conditionX.positiveCoefficient = 1;
            effect.conditionY.positiveCoefficient = -0.5;

            var conditions = DirectInputFfbMapper.ToParameters(effect, TwoAxes).Parameters.AsConditionSet().Conditions;

            Assert.AreEqual(2, conditions.Length);
            Assert.AreEqual(10000, conditions[0].PositiveCoefficient);
            Assert.AreEqual(-5000, conditions[1].PositiveCoefficient);
        }

        [TestMethod]
        public void Infinite_duration_and_milliseconds_are_converted()
        {
            Assert.AreEqual(-1, DirectInputFfbMapper.ToParameters(new FfbEffect { type = FfbEffectType.Constant }, OneAxis).Duration);
            Assert.AreEqual(250000, DirectInputFfbMapper.ToParameters(new FfbEffect { type = FfbEffectType.Constant, duration = 250 }, OneAxis).Duration);
        }

        [TestMethod]
        public void Values_out_of_range_are_clamped()
        {
            Assert.AreEqual(10000, DirectInputFfbMapper.ToSigned(3));
            Assert.AreEqual(0, DirectInputFfbMapper.ToUnsigned(-1));
            Assert.AreEqual(35000, DirectInputFfbMapper.ToAngle(-10));
        }

        [TestMethod]
        public void Rumble_is_played_as_a_vibration_with_the_feel_of_the_strongest_motor()
        {
            var effect = new FfbEffect { type = FfbEffectType.Rumble, largeMotor = 0.3, smallMotor = 0.8 };

            var periodic = DirectInputFfbMapper.ToParameters(effect, OneAxis).Parameters.AsPeriodicForce();

            Assert.AreEqual(EffectGuid.Sine, DirectInputFfbMapper.ToGuid(FfbEffectType.Rumble));
            Assert.AreEqual(8000, periodic.Magnitude);
            Assert.AreEqual(20000, periodic.Period);
        }
    }
}
