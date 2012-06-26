using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using FreePIE.Core.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimDX.DirectInput;

namespace FreePIE.Tests.Core.Plugins
{
    [TestClass]
    public class When_writing_XY_neutral_values_to_virtual_joystick_device : PPJoyPluginTest
    {
        protected override void DoTest(PPJoyGlobal global)
        {
        }

        [TestMethod]
        public void Direct_xy_axis_should_be_centered()
        {
            Assert.AreEqual(0, state.X);
            Assert.AreEqual(0, state.Y);
        }
    }

    [TestClass]
    public class When_writing_XY_max_values_to_virtual_joystick_device : PPJoyPluginTest
    {
        protected override void DoTest(PPJoyGlobal global)
        {
            global.setAxis((int)AxisTypes.X, 1000);
            global.setAxis((int)AxisTypes.Y, 1000);
        }

        [TestMethod]
        public void Direct_xy_axis_should_be_at_max_range()
        {
            Assert.AreEqual(1000, state.X);
            Assert.AreEqual(1000, state.Y);
        }
    }

    [TestClass]
    public class When_writing_XY_min_values_to_virtual_joystick_device : PPJoyPluginTest
    {
        protected override void DoTest(PPJoyGlobal global)
        {
            global.setAxis((int)AxisTypes.X, -1000);
            global.setAxis((int)AxisTypes.Y, -1000);
        }

        [TestMethod]
        public void Direct_xy_axis_should_be_at_min_range()
        {
            Assert.AreEqual(-1000, state.X);
            Assert.AreEqual(-1000, state.Y);
        }
    }

    [TestClass]
    public class When_writing_XY_arbitrary_values_to_virtual_joystick_device : PPJoyPluginTest
    {
        protected override void DoTest(PPJoyGlobal global)
        {
            global.setAxis((int)AxisTypes.X, -450);
            global.setAxis((int)AxisTypes.Y, 450);
        }

        [TestMethod]
        public void Direct_xy_axis_should_be_at_arbitrary_range()
        {
            Assert.AreEqual(-450, state.X);
            Assert.AreEqual(450, state.Y);
        }
    }

    public abstract class PPJoyPluginTest
    {
        protected JoystickState state;
        protected abstract void DoTest(PPJoyGlobal global);

        protected PPJoyPluginTest()
        {
            var plugin = new PPJoyPlugin();
            var global = (plugin.CreateGlobal() as PPJoyGlobal[])[0];

            plugin.Start();

            using (var joystick = GetJoystick())
            {
                if (joystick == null)
                    Assert.Fail("No PPJoy virtual stick found");

                DoTest(global);
                plugin.DoBeforeNextExecute();

                while((joystick.Acquire().IsFailure || joystick.Poll().IsFailure)) { }

                state = joystick.GetCurrentState();
            }

            plugin.Stop();
        }

        private Joystick GetJoystick()
        {
            var directInput = new DirectInput();
            var form = new Form();

            foreach (var device in directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
            {
                var controller = new Joystick(directInput, device.InstanceGuid);
                controller.SetCooperativeLevel(form.Handle, CooperativeLevel.Exclusive | CooperativeLevel.Background);

                var retries = 0;
                while (controller.Acquire().IsFailure)
                {
                    retries++;
                    if (retries > 500)
                        throw new Exception("Couldnt acquire SlimDX stick");
                }

                if (controller.Information.InstanceName.Contains("PPJoy"))
                {

                    foreach (DeviceObjectInstance deviceObject in controller.GetObjects())
                    {
                        if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                            controller.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-1000, 1000);
                    }

                    return controller;
                }
            }

            return null;
        }
    }
}
