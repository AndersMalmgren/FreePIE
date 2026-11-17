using System;
using System.Collections.Generic;
using System.Linq;

using FreePIE.Core.Common.Extensions;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;
using FreePIE.Tests.Test;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core
{
    [TestClass]
    public class CurveHelperTests : TestBase
    {
        protected CurveHelper curveHelper;

        public CurveHelperTests()
        {
            curveHelper = Get<CurveHelper>();
        }

        [TestMethod]
        public void It_should_return_a_named_CurveGlobal()
        {
            var curve = curveHelper.create(0,1);
            Assert.IsInstanceOfType(curve, typeof(CurveGlobalProvider.CurveGlobal));
            Assert.IsTrue(curve.Name.Length == 36);
        }

        [TestMethod]
        public void It_should_solve_for_y()
        {
            var curve = curveHelper.create(0,1);

            var y = curve.getY(0.5);

            Assert.AreEqual(0.5, y);
        }

        [TestMethod]
        public void It_should_ease_in_should_be_correct()
        {
            double min = 0;
            double max = 100;

            CurveGlobalProvider.CurveGlobal curve = curveHelper.create(min, max,60,12,93,70);

            var results = new Dictionary<double,double>();
            for(var i = min; i <= max; i+= (int)(max/10))
            {
                results.Add(i , Math.Round(curve.getY(i),2));
            }

            Assert.IsTrue(results.All(_ => _.Value >= min && _.Value <= max));
            
            Assert.AreEqual(results.First().Key , results.First().Value);
            Assert.AreEqual(results.Last().Key , results.Last().Value);

            Assert.IsTrue(results.Skip(1).TakeAllButLast().All(_ => _.Value < _.Key));
            

            
        }

        [TestMethod]
        public void It_should_ease_out_should_be_correct()
        {
            double min = 0;
            double max = 100;

            CurveGlobalProvider.CurveGlobal curve = curveHelper.create(min, max, 12, 40, 50, 91);

            var results = new Dictionary<double, double>();
            for (var i = min; i <= max; i += (int)(max / 10))
            {
                results.Add(i, Math.Round(curve.getY(i), 2));
            }

            Assert.IsTrue(results.All(_ => _.Value >= min && _.Value <= max));

            Assert.AreEqual(results.First().Key, results.First().Value);
            Assert.AreEqual(results.Last().Key, results.Last().Value);

            Assert.IsTrue(results.Skip(1).TakeAllButLast().All(_ => _.Value > _.Key));



        }
    }
}
