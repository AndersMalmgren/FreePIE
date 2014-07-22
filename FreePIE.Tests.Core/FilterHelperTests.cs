using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core
{
    public class MapRangeTest : TestBase
    {
        protected double x;
        protected double xMin;
        protected double xMax;
        protected double yMin;
        protected double yMax;
        protected double y;

        protected FilterHelper filterHelper;

        protected MapRangeTest()
        {
            filterHelper = Get<FilterHelper>();
        }

        protected void Map(Func<double, double, double, double, double, double> mapRangeFunc)
        {
            y = mapRangeFunc(x, xMin, xMax, yMin, yMax);
        }
    }

    [TestClass]
    public class When_giving_ensureMapRange_a_min_value_out_of_range : MapRangeTest
    {
        [TestInitialize]
        public void Context()
        {
            x = -19;
            xMin = -5;
            xMax = 5;
            yMin = -15;
            yMax = 15;

            Map(filterHelper.ensureMapRange);
        }

        [TestMethod]
        public void It_should_return_yMin()
        {
            Assert.AreEqual(-15, y);
        }
    }

    [TestClass]
    public class When_giving_ensureMapRange_a_max_value_out_of_range : MapRangeTest
    {
        [TestInitialize]
        public void Context()
        {
            x = 17;
            xMin = -5;
            xMax = 5;
            yMin = -15;
            yMax = 15;

            Map(filterHelper.ensureMapRange);
        }

        [TestMethod]
        public void It_should_return_yMin()
        {
            Assert.AreEqual(15, y);
        }
    }

    [TestClass]
    public class When_giving_ensureMapRange_a_value_in_range : MapRangeTest
    {
        [TestInitialize]
        public void Context()
        {
            x = 3;
            xMin = -5;
            xMax = 5;
            yMin = -15;
            yMax = 15;

            Map(filterHelper.ensureMapRange);
        }

        [TestMethod]
        public void It_should_return_yMin()
        {
            Assert.AreEqual(9, y);
        }
    }
}
