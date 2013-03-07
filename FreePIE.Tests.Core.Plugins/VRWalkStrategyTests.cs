using System;
using FreePIE.Core.Plugins.Extensions;
using FreePIE.Core.Plugins.Strategies;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core.Plugins
{
    [TestClass]
    public class When_inputing_yaw_270_bearing_90_delta_two : TestBase
    {
        private VRWalkStrategy vrWalk;

        [TestInitialize]
        public void Context()
        {
            vrWalk = new VRWalkStrategy();
            vrWalk.Update(2, 90.0.Rad(), 270.0.Rad());
        }

        [TestMethod]
        public void It_should_calculate_a_y_delta_of_minus_two()
        {
            AssertDouble(-2, vrWalk.DeltaY);
        }

        [TestMethod]
        public void It_should_calculate_a_x_delta_of_zero()
        {
            AssertDouble(0, vrWalk.DeltaX);
        }
    }

    [TestClass]
    public class When_inputing_yaw_45_bearing_90_delta_two : TestBase
    {
        private VRWalkStrategy vrWalk;

        [TestInitialize]
        public void Context()
        {
            vrWalk = new VRWalkStrategy();
            vrWalk.Update(2, 90.0.Rad(), 45.0.Rad());
        }

        [TestMethod]
        public void It_should_calculate_that_you_have_moved_x_and_y_equal()
        {
            AssertDouble(vrWalk.DeltaX, vrWalk.DeltaY);
        }
    }
}
