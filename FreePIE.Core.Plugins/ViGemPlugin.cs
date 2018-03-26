using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(XboxGlobal), IsIndexed = true)]
    public class ViGemPlugin : Plugin
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

        private List<XboxGlobal> _globals;
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

        public void PlugController(int index)
        {
            if (!IsConnected || _errorOccured != ErrorState.OK)
                return;

            if (!_connectedControllers.Contains(index))
            {
                _connectedControllers.Add(index);
            }
        }

        public void UnPlugController(int index)
        {
            if (!IsConnected || _errorOccured != ErrorState.OK)
                return;

            if (_connectedControllers.Contains(index))
            {
                _connectedControllers.Remove(index);
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
                //TODO Log exception
                _errorOccured = ErrorState.OPEN_FAILED;
            }

            return base.Start();
        }

        public override void Stop()
        {
            _globals.ForEach(d =>
            {
                d.Dispose();
                UnPlugController(d.index);
            });

            _connectedControllers.Clear();
        }

        public override object CreateGlobal()
        {
            _globals = new List<XboxGlobal>();
            return new GlobalIndexer<XboxGlobal>(CreateGlobal);
        }

        private XboxGlobal CreateGlobal(int index)
        {
            var global = new XboxGlobal(index, this);
            _globals.Add(global);

            return global;
        }

        public override void DoBeforeNextExecute()
        {
            _globals.ForEach(d => d.Update());
        }

        public override string FriendlyName { get { return "ViGEm Virtual Bus"; } }

    }

    [Global(Name = "xbox")]
    public class XboxGlobal : IDisposable
    {
        public readonly int index = -1;

        private ViGemPlugin _plugin;

        private Xbox360Controller _controller;
        private Xbox360Report _report = new Xbox360Report();

        //public x360ButtonCollection button { get; }

        public XboxGlobal(int index, ViGemPlugin plugin)
        {
            _plugin = plugin;
            _controller = new Xbox360Controller(plugin.Client);
            _controller.FeedbackReceived += _controller_FeedbackReceived;
            _controller.Connect();

            this.index = index;

            _plugin.PlugController(index);

        }

        public event RumbleEvent onRumble;
        public delegate void RumbleEvent(byte largeMotor, byte smallMotor, byte led);

        private void _controller_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            onRumble?.Invoke(e.LargeMotor, e.SmallMotor, e.LedNumber);            
        }

        internal void Update()
        {
            if (_report != null)
            {
                _controller.SendReport(_report);
                //_prevReport = _report;
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
            set { _report.SetButtonState(Xbox360Buttons.A, value);         }
        }

        public bool b
        {
            get { return buttons.HasFlag(Xbox360Buttons.B); }
            set {
                _report.SetButtonState(Xbox360Buttons.B, value);
            }
        }

        public bool x
        {
            get { return  buttons.HasFlag(Xbox360Buttons.X);  }
            set {
                _report.SetButtonState(Xbox360Buttons.X, value);
            }
        }

        public bool y
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Y); }
            set {
                _report.SetButtonState(Xbox360Buttons.Y,value);
            }
        }

        public bool leftShoulder
        {
            get { return  buttons.HasFlag(Xbox360Buttons.LeftShoulder); }
            set {
                _report.SetButtonState(Xbox360Buttons.LeftShoulder,value);
            }
        }

        public bool rightShoulder
        {
            get { return  buttons.HasFlag(Xbox360Buttons.RightShoulder); }
            set {
                _report.SetButtonState(Xbox360Buttons.RightShoulder, value);
            }
        }

        public bool start
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Start); }
            set {
                _report.SetButtonState(Xbox360Buttons.Start, value);
            }
        }

        public bool back
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Back); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.Back, value);
            }
        }

        public bool up
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Up); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.Up, value);
            }
        }

        public bool down
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Down); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.Down, value);
            }
        }

        public bool left
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Left);  }
            set
            {
                _report.SetButtonState(Xbox360Buttons.Left, value);
            }
        }

        public bool right
        {
            get { return  buttons.HasFlag(Xbox360Buttons.Right); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.Right, value);
            }
        }

        public bool leftThumb
        {
            get { return buttons.HasFlag(Xbox360Buttons.LeftThumb); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.LeftThumb, value);
            }
        }

        public bool rightThumb
        {
            get { return buttons.HasFlag(Xbox360Buttons.RightThumb); }
            set
            {
                _report.SetButtonState(Xbox360Buttons.RightThumb, value);
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
                _report.SetAxis(Xbox360Axes.LeftThumbY, (short)ensureMapRange(value, -1, 1, -32768, 32767));
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
                _report.SetAxis(Xbox360Axes.RightThumbY, (short)ensureMapRange(value, -1, 1, -32768, 32767));
            }

        }


        private bool isBetween(double val, double min, double max, bool isInclusive = true)
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

        private double mapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return yMin + (yMax - yMin) * (x - xMin) / (xMax - xMin);
        }

        private double ensureMapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return Math.Max(Math.Min(((x - xMin) / (xMax - xMin)) * (yMax - yMin) + yMin, yMax), yMin);
        }

        public void Disconnect()
        {
            if (_controller != null)
            {
                _controller.Disconnect();
            }
        }
        public void Dispose()
        {
            if(_controller != null)
            {
                Disconnect();
                _controller.Dispose();
                _controller = null;
            }
        }
    }
    
}
