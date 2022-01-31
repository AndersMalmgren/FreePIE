using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimDX.DirectInput;

namespace FreePIE.Tests.Core.Plugins
{
    [TestClass]
    public class When_forwarding_ffb_data_to_a_joystick : TestBase
    {
        [TestInitialize]
        public void Context()
        {
            var form = new Form();
            Stub<IHandleProvider>();
            WhenAccessing<IHandleProvider, IntPtr>(x => x.Handle).Return(form.Handle);

            var devices = new DirectInput()
                .GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);

            var index = devices.IndexOf(devices.First(j => !j.InstanceName.ToLower().Contains("vjoy")));
            var vjoyIndex = devices.IndexOf(devices.First(j => j.InstanceName.ToLower().Contains("vjoy")));

            var joyPlugin = Get<JoystickPlugin>();
            var vJoyPlugin = Get<VJoyPlugin>();


            var vJoyGlobal = (vJoyPlugin.CreateGlobal() as GlobalIndexer<VJoyGlobal, uint>)[0];

            var globals = (joyPlugin.CreateGlobal() as GlobalIndexer<JoystickGlobal, int>);
            var vJoyFfbControl = globals[vjoyIndex];
            var global = globals[index];

            ThreadPool.QueueUserWorkItem(obj =>
            {
                vJoyGlobal.registerFfbDevice(global);
                vJoyFfbControl.createEffect(1, FreePIE.Core.Plugins.VJoy.EffectType.ConstantForce, -1, new[] {1, 0});
                vJoyFfbControl.setConstantForce(1, 5000);
                vJoyFfbControl.operateEffect(1, FreePIE.Core.Plugins.VJoy.EffectOperation.Start, 0);

            });

            Thread.Sleep(5000); //Give the code sometime to send ffb data to real game device

            joyPlugin.Stop();
            vJoyPlugin.Stop();

        }

        [TestMethod]
        public void It_should_be_able_to_forward_ffb_event_to_joystick()
        {
            //Check if constant force is applied to wheel/gamepad
        }
    }
}
