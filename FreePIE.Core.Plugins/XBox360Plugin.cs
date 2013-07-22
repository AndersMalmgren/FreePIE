using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using SlimDX.XInput;

namespace FreePIE.Core.Plugins {

    [GlobalType(Type = typeof (XBox360PluginGlobal), IsIndexed = true)]
    public class XBox360Plugin : Plugin
    {

        private List<XBox360PluginGlobal> globals;

        public override object CreateGlobal()
        {
            globals = new List<XBox360PluginGlobal>();

            return new GlobalIndexer<XBox360PluginGlobal>(CreateGlobal);
        }

        private XBox360PluginGlobal CreateGlobal(int index)
        {
            var global = new XBox360PluginGlobal((UserIndex) index);
            globals.Add(global);

            return global;
        }

        public override void DoBeforeNextExecute()
        {
            globals.ForEach(d => d.Update());
        }

        public override string FriendlyName
        {
            get { return "XBox360 Controller"; }
        }
    }

    [Global(Name = "xbox360")]
    public class XBox360PluginGlobal
    {
        private Gamepad pad;
        private Controller controller;

        public XBox360PluginGlobal(UserIndex index)
        {
            controller = new Controller(index);
            pad = new Gamepad();
        }

        internal void Update()
        {
            if (controller.IsConnected) 
                pad = controller.GetState().Gamepad;
        }

        public bool a
        {
            get { return ((pad.Buttons & GamepadButtonFlags.A) != 0); }
        }

        public bool b
        {
            get { return ((pad.Buttons & GamepadButtonFlags.B) != 0); }
        }

        public bool x
        {
            get { return ((pad.Buttons & GamepadButtonFlags.X) != 0); }
        }

        public bool y
        {
            get { return ((pad.Buttons & GamepadButtonFlags.Y) != 0); }
        }

        public bool leftShoulder
        {
            get { return ((pad.Buttons & GamepadButtonFlags.LeftShoulder) != 0); }
        }

        public bool rightShoulder
        {
            get { return ((pad.Buttons & GamepadButtonFlags.RightShoulder) != 0); }
        }

        public bool start
        {
            get { return ((pad.Buttons & GamepadButtonFlags.Start) != 0); }
        }

        public bool back
        {
            get { return ((pad.Buttons & GamepadButtonFlags.Back) != 0); }
        }

        public bool up
        {
            get { return ((pad.Buttons & GamepadButtonFlags.DPadUp) != 0); }
        }

        public bool down
        {
            get { return ((pad.Buttons & GamepadButtonFlags.DPadDown) != 0); }
        }

        public bool left
        {
            get { return ((pad.Buttons & GamepadButtonFlags.DPadLeft) != 0); }
        }

        public bool right
        {
            get { return ((pad.Buttons & GamepadButtonFlags.DPadRight) != 0); }
        }

        public double leftTrigger
        {
            get { return (pad.LeftTrigger/255.0); }
        }

        public double rightTrigger
        {
            get { return (pad.RightTrigger/255.0); }
        }

        public bool leftThumb
        {
            get { return ((pad.Buttons & GamepadButtonFlags.LeftThumb) != 0); }
        }

        public bool rightThumb
        {
            get { return ((pad.Buttons & GamepadButtonFlags.RightThumb) != 0); }
        }

        public double leftStickX
        {
            get
            {
                if (pad.LeftThumbX < 0)
                    return pad.LeftThumbX/32768.0;
                else
                    return pad.LeftThumbX/32767.0;
            }
        }

        public double leftStickY
        {
            get
            {
                if (pad.LeftThumbY < 0)
                    return pad.LeftThumbY/32768.0;
                else
                    return pad.LeftThumbY/32767.0;
            }
        }

        public double rightStickX
        {
            get
            {
                if (pad.RightThumbX < 0)
                    return pad.RightThumbX/32768.0;
                else
                    return pad.RightThumbX/32767.0;
            }
        }

        public double rightStickY
        {
            get
            {
                if (pad.RightThumbY < 0)
                    return pad.RightThumbY/32768.0;
                else
                    return pad.RightThumbY/32767.0;
            }
        }
    }
}
