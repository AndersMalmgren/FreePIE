using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(DS4Global), IsIndexed = true)]
    public class DualShock4Plugin : ViGemPlugin
    {
        public override string FriendlyName { get { return "DualShock4 Virtual Bus"; } }
        public override object CreateGlobal()
        {
            _globals = new List<VigemGlobal>();
            return new GlobalIndexer<DS4Global>(CreateGlobal);
        }

        private DS4Global CreateGlobal(int index)
        {
            var global = new DS4Global(index, this);
            _globals.Add(global);
            PlugController(index);
            return global;
        }
    }


    [GlobalType(Type = typeof(XboxGlobal), IsIndexed = true)]
    public class XboxGamePadPlugin : ViGemPlugin
    {
        public override string FriendlyName { get { return "X360 Virtual Bus"; } }
        public override object CreateGlobal()
        {
            _globals = new List<VigemGlobal>();
            return new GlobalIndexer<XboxGlobal>(CreateGlobal);
        }

        private XboxGlobal CreateGlobal(int index)
        {
            var global = new XboxGlobal(index, this);
            _globals.Add(global);
            PlugController(index);
            return global;
        }
    }

    
    public abstract class ViGemPlugin : Plugin
    {
        

        /// <summary>
        /// Indicates what went wrong
        /// </summary>
        public enum ErrorState
        {
            OK = 0,
            OPEN_FAILED,
            CLOSE_FAILED,
            PLUG_FAILED,
            UNPLUG_FAILED,
            
        }

        protected List<VigemGlobal> _globals;
        public ViGEmClient Client { get; private set; }

        private List<int> _connectedControllers = new List<int>();
        /// <summary>
        /// global flips this if an error occured plugging in
        /// </summary>
        private ErrorState _errorOccured;

        private bool IsConnected
        {
            get
            {
                if (Client != null)
                    return true;

                return false;
            }
        }

        public override Action Start()
        {
            try
            {
                Client = new ViGEmClient();
            }
            catch (Exception x)
            {
                _errorOccured = ErrorState.OPEN_FAILED;
                throw new Exception("You must install the ViGEM Virtual Bus driver. See https://github.com/nefarius/ViGEm/wiki/Driver-Installation");
                
            }

            return base.Start();
        }

        public override void Stop()
        {
            _globals.ForEach(g =>
            {
                g.Disconnect();
                UnPlugController(g.index);
            });

            _connectedControllers.Clear();
        }

        

        public override void DoBeforeNextExecute()
        {
            _globals.ForEach(d => d.Update());
        }

        

        protected void PlugController(int index)
        {
            if (!IsConnected || _errorOccured != ErrorState.OK)
                return;

            if (!_connectedControllers.Contains(index))
            {
                _connectedControllers.Add(index);
            }
        }

        protected void UnPlugController(int index)
        {
            if (!IsConnected || _errorOccured != ErrorState.OK)
                return;

            if (_connectedControllers.Contains(index))
            {
                _connectedControllers.Remove(index);
            }
        }

    }

    public abstract class VigemGlobal 
    {
        //protected ViGemPlugin _plugin;
        public readonly int index = -1;
        protected List<VigemGlobal> _globals;

        public VigemGlobal(int index)//, ViGemPlugin plugin)
        {
            //_plugin = plugin;
            this.index = index;

        }

        protected bool isBetween(double val, double min, double max, bool isInclusive = true)
        {
            if (isInclusive)
            {
                return (val >= min) && (val <= max);
            }
            else
            {
                return (val > min) && (val < max);
            }
        }

        protected double mapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return yMin + (yMax - yMin) * (x - xMin) / (xMax - xMin);
        }

        protected double ensureMapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return Math.Max(Math.Min(((x - xMin) / (xMax - xMin)) * (yMax - yMin) + yMin, yMax), yMin);
        }

        internal abstract void Disconnect();

        internal abstract void Update();
    }

    [Global(Name = "xinput")]
    public class XboxGlobal : VigemGlobal, IDisposable
    {
        //private ViGemPlugin _plugin;

        private Xbox360Controller _controller;
        private Xbox360Report _report = new Xbox360Report();

        //public x360ButtonCollection button { get; }

        public XboxGlobal(int index, ViGemPlugin plugin) : base(index)//,plugin)
        {
            _controller = new Xbox360Controller(plugin.Client);
            _controller.FeedbackReceived += _controller_FeedbackReceived;
            _controller.Connect();
        }

        public event RumbleEvent onRumble;
        public delegate void RumbleEvent(byte largeMotor, byte smallMotor, byte led);

        private void _controller_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            onRumble?.Invoke(e.LargeMotor, e.SmallMotor, e.LedNumber);            
        }

        internal override void Update()
        {
            if (_report != null)
            {
                _controller.SendReport(_report);                
            }
            _report = new Xbox360Report();

        }
        
        private Xbox360Buttons[] enumToArray(Xbox360Buttons buttons)
        {
            var a = Enum.GetValues(typeof(Xbox360Buttons)).Cast<Xbox360Buttons>();
            var pressed = a.Where(b => (b & buttons) == b).ToArray();
            return pressed;
        }
        private Xbox360Buttons buttons
        {
            get { return (Xbox360Buttons)_report?.Buttons; }           
        }

        public bool a
        {
            get { return buttons.HasFlag(Xbox360Buttons.A); }
            set { if(value)_report.SetButtons(Xbox360Buttons.A); }
        }

        public bool b
        {
            get { return buttons.HasFlag(Xbox360Buttons.B); }
            set {
                if(value)_report.SetButtons(Xbox360Buttons.B);
            }
        }

        public bool x
        {
            get { return  buttons.HasFlag(Xbox360Buttons.X);  }
            set {
                if(value)_report.SetButtons(Xbox360Buttons.X);
            }
        }

        public bool y
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Y); }
            set {
                if(value)_report.SetButtons(Xbox360Buttons.Y);
            }
        }

        public bool leftShoulder
        {
            get { return  buttons.HasFlag(Xbox360Buttons.LeftShoulder); }
            set {
                if(value)_report.SetButtons(Xbox360Buttons.LeftShoulder);
            }
        }

        public bool rightShoulder
        {
            get { return  buttons.HasFlag(Xbox360Buttons.RightShoulder); }
            set {
                if(value)_report.SetButtons(Xbox360Buttons.RightShoulder);
            }
        }

        public bool start
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Start); }
            set {
                if(value)_report.SetButtons(Xbox360Buttons.Start);
            }
        }

        public bool back
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Back); }
            set
            {
                if(value)_report.SetButtons(Xbox360Buttons.Back);
            }
        }

        public bool up
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Up); }
            set
            {
                if(value)_report.SetButtons(Xbox360Buttons.Up);
            }
        }

        public bool down
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Down); }
            set
            {
                if(value)_report.SetButtons(Xbox360Buttons.Down);
            }
        }

        public bool left
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Left);  }
            set
            {
                if(value)_report.SetButtons(Xbox360Buttons.Left);
            }
        }

        public bool right
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Right); }
            set
            {
                if(value)_report.SetButtons(Xbox360Buttons.Right);
            }
        }

        public bool leftThumb
        {
            get { return buttons.HasFlag(Xbox360Buttons.LeftThumb); }
            set
            {
                if(value)_report.SetButtons(Xbox360Buttons.LeftThumb);
            }
        }

        public bool rightThumb
        {
            get { return buttons.HasFlag(Xbox360Buttons.RightThumb); }
            set
            {
                if(value)_report.SetButtons(Xbox360Buttons.RightThumb);
            }
        }

        /// <summary>
        /// Acceptable values range 0 - 1
        /// </summary>
        public double leftTrigger
        {
            get { return  _report.LeftTrigger/255.0; }
            set
            {
                if (isBetween(value, 0, 1))
                {
                    var v = (short)ensureMapRange(value, 0, 1, 0, 255);
                    _report.SetAxis(Xbox360Axes.LeftTrigger,v);
                }
            }
        }

        
        public double rightTrigger
        {
            get { return _report.RightTrigger/255.0; }
            set
            {
                if(isBetween(value,0,1))
                {
                    var v = (short)ensureMapRange(value, 0, 1, 0, 255);
                    _report.SetAxis(Xbox360Axes.RightTrigger, v);
                }                
            }
        }

        public double leftStickX
        {
            get {
                if (_report.LeftThumbX < 0)
                    return _report.LeftThumbX / 32768.0;
                else
                    return _report.LeftThumbX / 32767.0; 
            }
            set
            {
                _report.SetAxis(Xbox360Axes.LeftThumbX , (short)ensureMapRange(value, -1, 1, -32768, 32767)); 
            }
        }

        public double leftStickY
        {
            get
            {
                if (_report.LeftThumbX < 0)
                    return _report.LeftThumbY / 32768.0;
                else
                    return _report.LeftThumbY / 32767.0;
            }
            set
            {
                _report.SetAxis(Xbox360Axes.LeftThumbY, (short)ensureMapRange(value, 1, -1, -32768, 32767));
            }
        }

        public double rightStickX
        {
            get
            {
                if (_report.RightThumbX < 0)
                    return _report.RightThumbX / 32768.0;
                else
                    return _report.RightThumbX / 32767.0;
            }
            set
            {
                _report.SetAxis(Xbox360Axes.RightThumbX, (short)ensureMapRange(value, -1, 1, -32768, 32767));
            }

        }


        public double rightStickY
        {
            get
            {
                if (_report.RightThumbY < 0)
                    return _report.RightThumbY / 32768.0;
                else
                    return _report.RightThumbY / 32767.0;
            }
            set
            {
                _report.SetAxis(Xbox360Axes.RightThumbY, (short)ensureMapRange(value, 1, -1, -32768, 32767));
            }

        }


        

        internal override void Disconnect()
        {
            if (_controller != null)
            {
                _controller.Disconnect();
            }
        }
        void IDisposable.Dispose()
        {
            if(_controller != null)
            {
                Disconnect();
                _controller.Dispose();
                _controller = null;
            }
        }
    }


    [Global(Name = "dualshock")]
    public class DS4Global : VigemGlobal, IDisposable
    {             

        private DualShock4Controller _controller;
        private DualShock4Report _report;

        public DS4Global(int index, ViGemPlugin plugin) :base(index)
        {
            _report = new DualShock4Report();

            _controller = new DualShock4Controller(plugin.Client);
            _controller.FeedbackReceived += _controller_FeedbackReceived;
            _controller.Connect();
        }

        public event RumbleEvent onRumble;
        public delegate void RumbleEvent(byte largeMotor, byte smallMotor, LightbarColor led);

        private void _controller_FeedbackReceived(object sender, DualShock4FeedbackReceivedEventArgs e)
        {
            onRumble?.Invoke(e.LargeMotor, e.SmallMotor, e.LightbarColor);
        }

        internal override void Update()
        {
            if (_report != null)
            {
                if (_DpadFlags > 0)
                {
                    DualShock4DPadValues d = conv(_DpadFlags);

                    _report.SetDPad(d);
                }
                else
                {
                    //_report.SetDPad(DualShock4DPadValues.None);
                }
                _controller.SendReport(_report);                
            }
            _report = new DualShock4Report();
            leftStickX = 0;
            leftStickY = 0;
            rightStickX = 0;
            rightStickY = 0;

            

        }

        private DualShock4Buttons[] enumToArray(DualShock4Buttons buttons)
        {
            var a = Enum.GetValues(typeof(DualShock4Buttons)).Cast<DualShock4Buttons>();
            var pressed = a.Where(b => (b & buttons) == b).ToArray();
            return pressed;
        }
        private DualShock4Buttons buttons
        {
            get { return (DualShock4Buttons)_report?.Buttons; }
        }

        public bool cross
        {
            get { return buttons.HasFlag(DualShock4Buttons.Cross); }
            set { if(value)_report.SetButtons(DualShock4Buttons.Cross); }
        }

        public bool circle
        {
            get { return buttons.HasFlag(DualShock4Buttons.Circle); }
            set
            {
                if(value)_report.SetButtons(DualShock4Buttons.Circle);
            }
        }

        public bool square
        {
            get { return buttons.HasFlag(DualShock4Buttons.Square); }
            set
            {
                if(value)_report.SetButtons(DualShock4Buttons.Square);
            }
        }

        public bool triangle
        {
            get { return buttons.HasFlag(DualShock4Buttons.Triangle); }
            set
            {
                if(value)_report.SetButtons(DualShock4Buttons.Triangle);
            }
        }

        public bool L2
        {
            get { return buttons.HasFlag(DualShock4Buttons.ShoulderLeft); }
            set
            {
                if(value)_report.SetButtons(DualShock4Buttons.ShoulderLeft);
            }
        }

        public bool R2
        {
            get { return buttons.HasFlag(DualShock4Buttons.ShoulderRight); }
            set
            {
                if(value)_report.SetButtons(DualShock4Buttons.ShoulderRight);
            }
        }

        public bool options
        {
            get { return buttons.HasFlag(DualShock4Buttons.Options); }
            set
            {
                if(value)_report.SetButtons(DualShock4Buttons.Options);
            }
        }

        public bool share
        {
            get { return buttons.HasFlag(DualShock4Buttons.Share); }
            set
            {
                if(value)_report.SetButtons(DualShock4Buttons.Share);
            }
        }
        

        public bool up
        {
            get { return _DpadFlags.HasFlag(dpadFlags.Up); }
            set
            {
                if (value)
                    _DpadFlags |= dpadFlags.Up;
                else
                    _DpadFlags &= ~dpadFlags.Up;
                
            }
        }

        public bool down
        {
            get { return buttons.HasFlag(DualShock4DPadValues.South); }
            set
            {
                if (value)
                    _DpadFlags |= dpadFlags.Down;
                else
                    _DpadFlags &= ~dpadFlags.Down;
            }
        }

        public bool left
        {
            get { return buttons.HasFlag(DualShock4DPadValues.West); }
            set
            {
                if (value)
                    _DpadFlags |= dpadFlags.Left;
                else
                    _DpadFlags &= ~dpadFlags.Left;
            }
        }

        public bool right
        {
            get { return buttons.HasFlag(DualShock4DPadValues.East); }
            set
            {
                if (value)
                    _DpadFlags |= dpadFlags.Right;
                else
                    _DpadFlags &= ~dpadFlags.Right;
            }
        }

        public bool L3
        {
            get { return buttons.HasFlag(DualShock4Buttons.ThumbLeft); }
            set
            {
                if(value)_report.SetButtons(DualShock4Buttons.ThumbLeft);
            }
        }

        public bool R3
        {
            get { return buttons.HasFlag(DualShock4Buttons.ThumbRight); }
            set
            {
                if(value)_report.SetButtons(DualShock4Buttons.ThumbRight);
            }
        }

        public bool PS
        {
            get { return ((DualShock4SpecialButtons)_report.SpecialButtons).HasFlag(DualShock4SpecialButtons.Ps); }
            set
            {
                if(value)
                    _report.SetSpecialButtons(DualShock4SpecialButtons.Ps);
            }
        }

        public bool touchPad
        {
            get { return ((DualShock4SpecialButtons)_report.SpecialButtons).HasFlag(DualShock4SpecialButtons.Touchpad); }
            set
            {
                if (value)
                    _report.SetSpecialButtons(DualShock4SpecialButtons.Touchpad);
            }
        }

        /// <summary>
        /// Acceptable values range 0 - 1
        /// </summary>
        public double L1
        {
            get { return _report.LeftTrigger / 255.0; }
            set
            {
                if (isBetween(value, 0, 1))
                {
                    var v = (byte)ensureMapRange(value, 0, 1, 0, 255);
                    _report.SetAxis(DualShock4Axes.LeftTrigger, v);
                }
            }
        }


        public double R1
        {
            get { return _report.RightTrigger / 255.0; }
            set
            {
                if (isBetween(value, 0, 1))
                {
                    var v = (byte)ensureMapRange(value, 0, 1, 0, 255);
                    _report.SetAxis(DualShock4Axes.RightTrigger, v);
                }
            }
        }

        public double leftStickX
        {
            get
            {
                return mapRange(_report.LeftThumbX, 0, 255, -1, 1);
            }
            set
            {
                _report.SetAxis(DualShock4Axes.LeftThumbX,(byte) ensureMapRange(value, -1, 1, 0, 255)); //(short)ensureMapRange(value, -1, 1, -32768, 32767));
            }
        }

        public double leftStickY
        {
            get
            {
                return -mapRange(_report.LeftThumbY, 0, 255, -1, 1); 
            }
            set
            {
                _report.SetAxis(DualShock4Axes.LeftThumbY, (byte)ensureMapRange(value, 1, -1, 0, 255));
            }
        }

        public double rightStickX
        {
            get
            {
                return mapRange(_report.RightThumbX, 0, 255, -1, 1);              
            }
            set
            {
                _report.SetAxis(DualShock4Axes.RightThumbX, (byte)ensureMapRange(value, -1, 1, 0, 255));
            }

        }


        public double rightStickY
        {
            get
            {
                return -mapRange(_report.RightThumbY, 0, 255, -1, 1); 
            }
            set
            {
                _report.SetAxis(DualShock4Axes.RightThumbY, (byte)ensureMapRange(value, 1, -1, 0, 255));
            }

        }


        

        [Flags]
        private enum dpadFlags
        {
           None = 0,  Up = 1 << 0, Right = 1 << 1, Down = 1 << 2, Left = 1 << 3
        }

        dpadFlags _DpadFlags = 0;

        private static DualShock4DPadValues conv(dpadFlags flags)
        {

            DualShock4DPadValues retval = DualShock4DPadValues.None;

            var g = new List<KeyValuePair<dpadFlags, DualShock4DPadValues>>()
            {
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Up,DualShock4DPadValues.North),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Up | dpadFlags.Right,DualShock4DPadValues.Northeast ),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Right, DualShock4DPadValues.East ),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Right | dpadFlags.Down,DualShock4DPadValues.Southeast),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Down, DualShock4DPadValues.South),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Down | dpadFlags.Left, DualShock4DPadValues.Southwest),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Left, DualShock4DPadValues.West),
                new KeyValuePair<dpadFlags, DualShock4DPadValues>( dpadFlags.Up | dpadFlags.Left, DualShock4DPadValues.Northwest)
            };
            //Console.WriteLine(g.Aggregate((a,c) => string.Format("{0},{1}",a,c)));
            try
            {
                if (flags != 0)
                    retval = g.Single(gg => gg.Key == flags).Value;

            }
            catch
            {

                
            }

            return retval;

        }

        internal override void Disconnect()
        {
            if (_controller != null)
            {
                _controller.Disconnect();
            }
        }
        
        void IDisposable.Dispose()
        {
            if (_controller != null)
            {
                Disconnect();
                _controller.Dispose();
                _controller = null;
            }
        }
    }
}
