using System;
using System.Globalization;
using System.Threading;
using FreePIE.Core.Model;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;
using FreePIE.Core.ScriptEngine.Python;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core
{
    [TestClass]
    public class CurveHelperTests
    {
        private ScriptEngine engine;
        private ScriptScope scope;

        [TestInitialize]
        public void Context()
        {
            engine = IronPython.Hosting.Python.CreateEngine();
            scope = engine.CreateScope();
            scope.SetVariable("curves", new CurveHelper());
        }

        private CurveGlobalProvider.CurveGlobal Execute(string script)
        {
            return engine.Execute<CurveGlobalProvider.CurveGlobal>(script, scope);
        }

        private void AssertInvalid(string script, string expectedMessage)
        {
            try
            {
                Execute(script);
            }
            catch (ArgumentException e)
            {
                StringAssert.Contains(e.Message, expectedMessage);
                return;
            }

            Assert.Fail("Expected an ArgumentException for " + script);
        }

        [TestMethod]
        public void A_curve_can_be_created_from_a_python_list_of_tuples()
        {
            var curve = Execute("curves.create([(0, 0), (12, 40), (50, 91), (100, 100)])");

            Assert.AreEqual(0, curve.getY(0), 0.0001);
            Assert.AreEqual(40, curve.getY(12), 0.0001);
            Assert.AreEqual(91, curve.getY(50), 0.0001);
            Assert.AreEqual(100, curve.getY(100), 0.0001);
        }

        [TestMethod]
        public void A_curve_can_be_created_from_lists_and_generated_points()
        {
            Assert.AreEqual(40, Execute("curves.create([[0, 0], [12.5, 40], [100, 100]])").getY(12.5), 0.0001);
            Assert.AreEqual(25, Execute("curves.create([(x, x * x / 100.0) for x in range(0, 101, 25)])").getY(50), 0.0001);
        }

        [TestMethod]
        public void Ease_in_and_ease_out_curves_stay_below_and_above_the_linear_curve()
        {
            var easeIn = Execute("curves.create([(0, 0), (60, 12), (93, 70), (100, 100)])");
            var easeOut = Execute("curves.create([(0, 0), (12, 40), (50, 91), (100, 100)])");

            for (var x = 10; x < 100; x += 10)
            {
                Assert.IsTrue(easeIn.getY(x) < x, "Ease in at " + x);
                Assert.IsTrue(easeOut.getY(x) > x, "Ease out at " + x);
            }
        }

        [TestMethod]
        public void Each_point_must_be_a_pair()
        {
            AssertInvalid("curves.create([(0, 0), (50,), (100, 100)])", "pair");
            AssertInvalid("curves.create([0, 50, 100])", "pair");
        }

        [TestMethod]
        public void Point_values_must_be_numbers()
        {
            AssertInvalid("curves.create([(0, 0), ('a', 50), (100, 100)])", "numbers");
        }

        [TestMethod]
        public void A_curve_needs_at_least_two_points()
        {
            AssertInvalid("curves.create([(0, 0)])", "two points");
        }

        [TestMethod]
        public void Points_must_be_sorted_on_x_without_duplicates()
        {
            AssertInvalid("curves.create([(0, 0), (80, 20), (20, 80), (100, 100)])", "sorted");
            AssertInvalid("curves.create([(0, 0), (50, 20), (50, 80), (100, 100)])", "sorted");
        }

        [TestMethod]
        public void A_curve_copied_as_script_creates_the_same_curve_regardless_of_culture()
        {
            var original = new Curve("Ease out", new[] { new Point(0, 10), new Point(12.5, 40.25), new Point(50, 91), new Point(100, 95) });
            var originalGlobal = new CurveGlobalProvider.CurveGlobal(original);

            var culture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("sv-SE");
                var script = CurveScript.Create(original);

                Assert.AreEqual("Ease_out = curves.create([(0, 10), (12.5, 40.25), (50, 91), (100, 95)])", script);

                engine.Execute(script, scope);
                var copy = scope.GetVariable<CurveGlobalProvider.CurveGlobal>("Ease_out");

                for (var x = 0; x <= 100; x += 5)
                    Assert.AreEqual(originalGlobal.getY(x), copy.getY(x), 0.0001);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        [TestMethod]
        public void A_curve_name_that_is_not_a_python_identifier_is_made_into_one()
        {
            var points = new[] { new Point(0, 0), new Point(100, 100) };

            StringAssert.StartsWith(CurveScript.Create(new Curve("1st curve", points)), "_1st_curve = ");
            StringAssert.StartsWith(CurveScript.Create(new Curve(points)), "curve = ");
        }
    }
}
