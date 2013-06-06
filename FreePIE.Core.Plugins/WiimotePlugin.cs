using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.TrackIR;
using FreePIE.Core.Plugins.Wiimote;
using System.Linq;

namespace FreePIE.Core.Plugins
{
    [GlobalEnum]
    public enum WiimoteButtons : ushort
    {
        DPadLeft = 0x0100,
        DPadRight = 0x0200,
        DPadDown = 0x0400,
        DPadUp = 0x0800,
        A = 0x0008,
        B = 0x0004,
        Plus = 0x1000,
        Minus = 0x0010,
        One = 0x0002,
        Two = 0x0001,
        Home = 0x080
    }

    [GlobalEnum]
    public enum WiimoteCapabilities : ushort
    {
        None = 0,
        MotionPlus = 2,
        Extension = 4,
        IR = 8
    }

    public enum WiimoteDataValid
    {
        Acceleration = 0x0002,
        MotionPlus = 0x0004, 
        Nunchuck = 0x0008
    }

    public enum WiimoteExtensions
    {
        None = 0,
        Nunchuck = 1,
        ClassicController = 2,
        ClassicControllerPro = 4,
        GuitarHeroGuitar = 8,
        GuitarHeroDrums = 16,
        MotionPlus = 32
    }

    [GlobalType(Type = typeof(WiimoteGlobal), IsIndexed = true)]
    public class WiimotePlugin : Plugin
    {
        private LogLevel logLevel;
        private bool doLog;
        private IWiimoteBridge wiimoteBridge;
        private Dictionary<uint, Action> globalUpdators;
        private IList<uint> updatedWiimotes; 

        public override object CreateGlobal()
        {
            wiimoteBridge = new DolphiimoteBridge(logLevel, doLog ? "dolphiimote.log" : null);
            globalUpdators = new Dictionary<uint, Action>();
            updatedWiimotes = new List<uint>();
            return Enumerable.Range(0, 4).Select(i => new WiimoteGlobal(this, wiimoteBridge.GetData((uint)i), globalUpdators)).ToArray();
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            if (index > 1)
                return false;
            if (index == 0)
            {
                property.Name = "LogLevel";
                property.Caption = "Log level";

                foreach (var val in Enum.GetNames(typeof (LogLevel)))
                    property.Choices.Add(val, val);

                property.DefaultValue = LogLevel.Warning.ToString();
                property.HelpText = "Set the log level for Dolphiimote library";
            }

            if (index == 1)
            {
                property.Name = "DoLog";
                property.Caption = "Enable logging";
                property.DefaultValue = false;
                property.HelpText = "Enable logging for Dolphiimote library. Look for dolphiimote.log in the application directory.";
            }

            return true;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), (string)properties["LogLevel"]);
            doLog = (bool)properties["DoLog"];
            return true;
        }

        public override Action Start()
        {
            wiimoteBridge.DataReceived += WiimoteDataReceived;
            wiimoteBridge.Init();
            return null;
        }

        public void Enable(byte wiimote, WiimoteCapabilities flags)
        {
            wiimoteBridge.Enable(wiimote, flags);
        }

        private void WiimoteDataReceived(object sender, UpdateEventArgs<uint> updated)
        {
            updatedWiimotes.Add(updated.UpdatedValue);
        }

        public override void Stop()
        {
            wiimoteBridge.Dispose();
        }

        public override string FriendlyName
        {
            get { return "Wiimote"; }
        }

        public override void DoBeforeNextExecute()
        {
            updatedWiimotes.Clear();
            wiimoteBridge.DoTick();
            foreach(var wiimote in updatedWiimotes)
                globalUpdators[wiimote]();
        }
    }

    [Global(Name = "wiimote")]
    public class WiimoteGlobal
    {
        private readonly WiimotePlugin plugin;
        private readonly IWiimoteData data;

        private readonly Action accelerationTrigger;
        private readonly Action buttonTrigger;
        private readonly Action motionPlusTrigger;

        public WiimoteGlobal(WiimotePlugin plugin, IWiimoteData data, Dictionary<uint, Action> updaters)
        {
            this.plugin = plugin;
            this.data = data;

            acceleration = new Acceleration(data, out accelerationTrigger);
            buttons = new WiimoteButtonState(data, out buttonTrigger);
            motionplus = new MotionPlus(data, out motionPlusTrigger);

            updaters[data.WiimoteNumber] = OnWiimoteDataReceived;
        }

        public void enable(WiimoteCapabilities flags)
        {
            plugin.Enable(data.WiimoteNumber, flags);
        }

        private void OnWiimoteDataReceived()
        {
            buttonTrigger();

            if(data.IsDataValid(WiimoteDataValid.Acceleration))
                accelerationTrigger();

            if (data.IsDataValid(WiimoteDataValid.MotionPlus))
                motionPlusTrigger();
        }

        public MotionPlus motionplus
        { 
            get;
            private set;
        }

        public EulerAngles ahrs
        {
            get { return data.MotionPlusEulerAngles; }
        }

        public Acceleration acceleration
        {
            get;
            private set;
        }

        public WiimoteButtonState buttons
        { 
            get;
            private set;
        }
    }
}
