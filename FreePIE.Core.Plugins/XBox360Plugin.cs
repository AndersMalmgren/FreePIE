using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Ffb;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.Strategies;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Exceptions;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using SlimDX.XInput;

namespace FreePIE.Core.Plugins {

    // Values are the XInput button flags, which ViGEm uses as well
    [GlobalEnum]
    [Documentation("Buttons of an Xbox 360 controller")]
    public enum XBox360Button
    {
        [Documentation("D-pad up")]
        Up = 0x0001,
        [Documentation("D-pad down")]
        Down = 0x0002,
        [Documentation("D-pad left")]
        Left = 0x0004,
        [Documentation("D-pad right")]
        Right = 0x0008,
        Start = 0x0010,
        Back = 0x0020,
        [Documentation("Left stick pressed in")]
        LeftThumb = 0x0040,
        [Documentation("Right stick pressed in")]
        RightThumb = 0x0080,
        LeftShoulder = 0x0100,
        RightShoulder = 0x0200,
        [Documentation("Guide button, can only be emulated, XInput does not report it")]
        Guide = 0x0400,
        A = 0x1000,
        B = 0x2000,
        X = 0x4000,
        Y = 0x8000
    }

    [GlobalType(Type = typeof (XBox360PluginGlobal), IsIndexed = true)]
    public class XBox360Plugin : Plugin
    {

        private List<XBox360PluginGlobal> globals;
        private ViGEmClient client;

        public override object CreateGlobal()
        {
            globals = new List<XBox360PluginGlobal>();

            return new GlobalIndexer<XBox360PluginGlobal>(CreateGlobal);
        }

        private XBox360PluginGlobal CreateGlobal(int index)
        {
            var global = new XBox360PluginGlobal((UserIndex) index, CreateVirtualController);
            globals.Add(global);

            return global;
        }

        private IXbox360Controller CreateVirtualController()
        {
            if (client == null)
            {
                try
                {
                    client = new ViGEmClient();
                }
                catch (VigemBusNotFoundException)
                {
                    throw new Exception("ViGEmBus driver not found, it must be installed to emulate Xbox 360 controllers");
                }
            }

            var controller = client.CreateXbox360Controller();
            controller.AutoSubmitReport = false;
            controller.Connect();

            return controller;
        }

        public override void DoBeforeNextExecute()
        {
            globals.ForEach(d => d.Update());
        }

        public override void Stop()
        {
            globals.ForEach(g => g.Dispose());

            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }

        public override string FriendlyName
        {
            get { return "XBox360 Controller"; }
        }
    }

    [Global(Name = "xbox360")]
    [Documentation("Reads an Xbox 360 or other XInput controller, index by player, xbox360[0] is the first controller. Writing to it emulates a virtual Xbox 360 controller through ViGEmBus, it shows up in the first free XInput slot")]
    public class XBox360PluginGlobal : IDisposable
    {
        private Gamepad pad;
        private readonly Controller controller;
        private readonly Func<IXbox360Controller> createVirtualController;
        private readonly GetPressedStrategy<XBox360Button> getPressedStrategy;
        private readonly SetPressedStrategy<XBox360Button> setPressedStrategy;

        private IXbox360Controller virtualController;
        private bool virtualStateChanged;
        private ushort virtualButtons;

        private RumbleFfbDevice rumbleDevice;
        private XBox360FfbGlobal ffbGlobal;

        public XBox360PluginGlobal(UserIndex index, Func<IXbox360Controller> createVirtualController)
        {
            controller = new Controller(index);
            pad = new Gamepad();
            this.createVirtualController = createVirtualController;
            getPressedStrategy = new GetPressedStrategy<XBox360Button>(IsDown);
            setPressedStrategy = new SetPressedStrategy<XBox360Button>(b => SetVirtualButton(b, true), b => SetVirtualButton(b, false));
        }

        internal void Update()
        {
            if (controller.IsConnected)
                pad = controller.GetState().Gamepad;

            setPressedStrategy.Do();
            SubmitVirtualState();

            if (ffbGlobal != null)
            {
                ffbGlobal.Pump();
                ffbGlobal.Flush();
                rumbleDevice.Render();
            }
        }

        public void Dispose()
        {
            if (rumbleDevice != null)
                rumbleDevice.Dispose();

            if (virtualController == null)
                return;

            virtualController.Disconnect();
            virtualController = null;
        }

        private void SetVibration(double large, double small)
        {
            if (!controller.IsConnected)
                return;

            controller.SetVibration(new Vibration
            {
                LeftMotorSpeed = (ushort) Math.Round(large * ushort.MaxValue),
                RightMotorSpeed = (ushort) Math.Round(small * ushort.MaxValue)
            });
        }

        private FfbSource CreateFfbSource()
        {
            var source = new FfbSource();
            var started = false;

            // Raised on a ViGEm thread, the source queues it for the script thread
            VirtualController.FeedbackReceived += (sender, e) =>
            {
                source.UpdateEffect(0, effect =>
                {
                    effect.type = FfbEffectType.Rumble;
                    effect.largeMotor = e.LargeMotor / 255.0;
                    effect.smallMotor = e.SmallMotor / 255.0;
                });

                if (started)
                    return;

                started = true;
                source.StartEffect(0, 1, false);
            };

            return source;
        }

        [Documentation("Rumble of the controller, it can rumble the physical controller and forward rumble from games to the emulated controller")]
        public XBox360FfbGlobal ffb
        {
            get
            {
                if (ffbGlobal != null)
                    return ffbGlobal;

                rumbleDevice = new RumbleFfbDevice(SetVibration);
                return ffbGlobal = new XBox360FfbGlobal(rumbleDevice, CreateFfbSource);
            }
        }

        private bool IsDown(XBox360Button button)
        {
            return ((ushort) pad.Buttons & (ushort) button) != 0;
        }

        private IXbox360Controller VirtualController
        {
            get { return virtualController ?? (virtualController = createVirtualController()); }
        }

        private void SetVirtualButton(XBox360Button button, bool pressed)
        {
            var flags = virtualButtons;
            if (pressed)
                flags |= (ushort) button;
            else
                flags &= (ushort) ~(ushort) button;

            if (flags == virtualButtons && virtualController != null)
                return;

            virtualButtons = flags;
            VirtualController.SetButtonsFull(flags);
            virtualStateChanged = true;
        }

        private void SetVirtualAxis(Xbox360Axis axis, double value)
        {
            value = Math.Max(-1, Math.Min(1, value));
            var scaled = value < 0 ? value * 32768 : value * 32767;

            VirtualController.SetAxisValue(axis, (short) Math.Round(scaled));
            virtualStateChanged = true;
        }

        private void SetVirtualTrigger(Xbox360Slider trigger, double value)
        {
            value = Math.Max(0, Math.Min(1, value));

            VirtualController.SetSliderValue(trigger, (byte) Math.Round(value * 255));
            virtualStateChanged = true;
        }

        // Sent once per iteration instead of on every change, each submit is a driver call
        private void SubmitVirtualState()
        {
            if (!virtualStateChanged)
                return;

            virtualController.SubmitReport();
            virtualStateChanged = false;
        }

        [Documentation("True while A is down")]
        public bool a
        {
            get { return ((pad.Buttons & GamepadButtonFlags.A) != 0); }
            set { SetVirtualButton(XBox360Button.A, value); }
        }

        [Documentation("True while B is down")]
        public bool b
        {
            get { return ((pad.Buttons & GamepadButtonFlags.B) != 0); }
            set { SetVirtualButton(XBox360Button.B, value); }
        }

        [Documentation("True while X is down")]
        public bool x
        {
            get { return ((pad.Buttons & GamepadButtonFlags.X) != 0); }
            set { SetVirtualButton(XBox360Button.X, value); }
        }

        [Documentation("True while Y is down")]
        public bool y
        {
            get { return ((pad.Buttons & GamepadButtonFlags.Y) != 0); }
            set { SetVirtualButton(XBox360Button.Y, value); }
        }

        [Documentation("True while the left shoulder button is down")]
        public bool leftShoulder
        {
            get { return ((pad.Buttons & GamepadButtonFlags.LeftShoulder) != 0); }
            set { SetVirtualButton(XBox360Button.LeftShoulder, value); }
        }

        [Documentation("True while the right shoulder button is down")]
        public bool rightShoulder
        {
            get { return ((pad.Buttons & GamepadButtonFlags.RightShoulder) != 0); }
            set { SetVirtualButton(XBox360Button.RightShoulder, value); }
        }

        [Documentation("True while Start is down")]
        public bool start
        {
            get { return ((pad.Buttons & GamepadButtonFlags.Start) != 0); }
            set { SetVirtualButton(XBox360Button.Start, value); }
        }

        [Documentation("True while Back is down")]
        public bool back
        {
            get { return ((pad.Buttons & GamepadButtonFlags.Back) != 0); }
            set { SetVirtualButton(XBox360Button.Back, value); }
        }

        [Documentation("True while the d-pad is pressed up")]
        public bool up
        {
            get { return ((pad.Buttons & GamepadButtonFlags.DPadUp) != 0); }
            set { SetVirtualButton(XBox360Button.Up, value); }
        }

        [Documentation("True while the d-pad is pressed down")]
        public bool down
        {
            get { return ((pad.Buttons & GamepadButtonFlags.DPadDown) != 0); }
            set { SetVirtualButton(XBox360Button.Down, value); }
        }

        [Documentation("True while the d-pad is pressed left")]
        public bool left
        {
            get { return ((pad.Buttons & GamepadButtonFlags.DPadLeft) != 0); }
            set { SetVirtualButton(XBox360Button.Left, value); }
        }

        [Documentation("True while the d-pad is pressed right")]
        public bool right
        {
            get { return ((pad.Buttons & GamepadButtonFlags.DPadRight) != 0); }
            set { SetVirtualButton(XBox360Button.Right, value); }
        }

        [Documentation("Left trigger, 0 to 1")]
        public double leftTrigger
        {
            get { return (pad.LeftTrigger/255.0); }
            set { SetVirtualTrigger(Xbox360Slider.LeftTrigger, value); }
        }

        [Documentation("Right trigger, 0 to 1")]
        public double rightTrigger
        {
            get { return (pad.RightTrigger/255.0); }
            set { SetVirtualTrigger(Xbox360Slider.RightTrigger, value); }
        }

        [Documentation("True while the left stick is pressed in")]
        public bool leftThumb
        {
            get { return ((pad.Buttons & GamepadButtonFlags.LeftThumb) != 0); }
            set { SetVirtualButton(XBox360Button.LeftThumb, value); }
        }

        [Documentation("True while the right stick is pressed in")]
        public bool rightThumb
        {
            get { return ((pad.Buttons & GamepadButtonFlags.RightThumb) != 0); }
            set { SetVirtualButton(XBox360Button.RightThumb, value); }
        }

        [Documentation("Left stick horizontal axis, -1 to 1")]
        public double leftStickX
        {
            get
            {
                if (pad.LeftThumbX < 0)
                    return pad.LeftThumbX/32768.0;
                else
                    return pad.LeftThumbX/32767.0;
            }
            set { SetVirtualAxis(Xbox360Axis.LeftThumbX, value); }
        }

        [Documentation("Left stick vertical axis, -1 to 1, up is positive")]
        public double leftStickY
        {
            get
            {
                if (pad.LeftThumbY < 0)
                    return pad.LeftThumbY/32768.0;
                else
                    return pad.LeftThumbY/32767.0;
            }
            set { SetVirtualAxis(Xbox360Axis.LeftThumbY, value); }
        }

        [Documentation("Right stick horizontal axis, -1 to 1")]
        public double rightStickX
        {
            get
            {
                if (pad.RightThumbX < 0)
                    return pad.RightThumbX/32768.0;
                else
                    return pad.RightThumbX/32767.0;
            }
            set { SetVirtualAxis(Xbox360Axis.RightThumbX, value); }
        }

        [Documentation("Right stick vertical axis, -1 to 1, up is positive")]
        public double rightStickY
        {
            get
            {
                if (pad.RightThumbY < 0)
                    return pad.RightThumbY/32768.0;
                else
                    return pad.RightThumbY/32767.0;
            }
            set { SetVirtualAxis(Xbox360Axis.RightThumbY, value); }
        }

        [Documentation("True while the button is down")]
        public bool getDown(XBox360Button button)
        {
            return IsDown(button);
        }

        [Documentation("Presses or releases the button on the emulated controller")]
        public void setDown(XBox360Button button, bool pressed)
        {
            SetVirtualButton(button, pressed);
        }

        [Documentation("True once when the button goes down, false until it has been released and pressed again")]
        public bool getPressed(XBox360Button button)
        {
            return getPressedStrategy.IsPressed(button);
        }

        [Documentation("Clicks the button on the emulated controller, it is pressed now and released in the next iteration")]
        public void setPressed(XBox360Button button)
        {
            setPressedStrategy.Add(button);
        }

        [Documentation("Clicks the button on the emulated controller once each time state changes from false to true")]
        public void setPressed(XBox360Button button, bool state)
        {
            setPressedStrategy.Add(button, state);
        }
    }
}
