using System;
using System.Collections.Generic;
using System.Diagnostics;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using JonesCorp;
using JonesCorp.Data;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(ScpPluginGlobal), IsIndexed = true)]
    public class ScpPlugin : Plugin
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

        private List<ScpPluginGlobal> _globals;

        private ScpDriver _scpDriver;

        private List<int> _connectedControllers = new List<int>();
        /// <summary>
        /// global flips this if an error occured plugging in
        /// </summary>
        private ErrorState _errorOccured;

        private bool IsConnected
        {
            get
            {
                if (_scpDriver != null && _scpDriver.CurrentState != ConnectionState.Disconnected)
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
                if (_scpDriver.Plugin(index + 1))
                {
                    _connectedControllers.Add(index);
                }
                else
                    _errorOccured = ErrorState.PLUG_FAILED;
            }
        }

        public void UnPlugController(int index)
        {
            if (!IsConnected || _errorOccured != ErrorState.OK)
                return;

            if (_connectedControllers.Contains(index))
            {
                if(_scpDriver.Unplug(index + 1))
                    _connectedControllers.Remove(index);
                else
                {
                    _errorOccured = ErrorState.UNPLUG_FAILED;
                }
            }
        }

        public void SendReport(SCP_XINPUT_DATA data, byte[] rumble)
        {
            if(IsConnected && _errorOccured == ErrorState.OK)
                _scpDriver.Report(data, rumble);
        }

        public override Action Start()
        {
            _scpDriver = new ScpDriver();
            if (!_scpDriver.Open())
            {
                _errorOccured = ErrorState.OPEN_FAILED;
            }

#if DEBUG
            Debug.Assert(_errorOccured == ErrorState.OK, "Error opening scp driver");
#endif
            return base.Start();
        }

        public override void Stop()
        {
            _globals.ForEach(d =>
            {
                UnPlugController(d.index);
            });

            if (_scpDriver.Close())
            {
                _connectedControllers.Clear();
            }
            else 
                _errorOccured = ErrorState.CLOSE_FAILED;
        }

        public override object CreateGlobal()
        {
            _globals = new List<ScpPluginGlobal>();

            return new GlobalIndexer<ScpPluginGlobal>(CreateGlobal);
        }

        private ScpPluginGlobal CreateGlobal(int index)
        {
            var global = new ScpPluginGlobal(index, this);
            _globals.Add(global);

            return global;
        }

        public override void DoBeforeNextExecute()
        {
            _globals.ForEach(d => d.Update());
        }

        public override string FriendlyName { get { return "Scp Virtual Bus"; } }

    }

    [Global(Name = "scp")]
    public class ScpPluginGlobal
    {
        public readonly int index = -1;

        private ScpPlugin _scpPlugin;

        private SCP_XINPUT_DATA _data, _pdata;

        //public x360ButtonCollection button { get; }

        public ScpPluginGlobal(int index, ScpPlugin plugin)
        {
            _scpPlugin = plugin;

            _data = new SCP_XINPUT_DATA
            {
                code1 = 0x1C,
                ControllerNumber = index + 1,
                Code2 = 0x14
            };

            //button = new x360ButtonCollection();
            
            _scpPlugin.PlugController(index);
             this.index = index;
            

#if DEBUG
            System.Diagnostics.Debug.Assert(this.index > -1, string.Format("Failed to plugin virtual xInput controller {0}", this.index));
#endif
        }

        internal void Update()
        {
            byte[] rumble = new byte[2];

            var different = !_pdata.Equals(_data);

            if (different)
            {

                _scpPlugin.SendReport(_data, rumble);

                _pdata = _data;

                _data = new SCP_XINPUT_DATA
                {
                    code1 = 0x1C,
                    ControllerNumber = index + 1,
                    Code2 = 0x14
                };
            }
        }
        

        public bool a
        {
            get { return _data.Buttons.HasFlag(X360Button.A); }
            set
            {
                if (value)
                    _data.Buttons |= X360Button.A;
                else
                    _data.Buttons &= ~X360Button.A;
            }
        }

        public bool b
        {
            get { return  _data.Buttons.HasFlag(X360Button.B); }
            set {
               
                if (value)
                    _data.Buttons |= X360Button.B;
                else
                    _data.Buttons &= ~X360Button.B;
            }
        }

        public bool x
        {
            get { return  _data.Buttons.HasFlag(X360Button.X);  }
            set {
                
                if (value)
                    _data.Buttons |= X360Button.X;
                else
                    _data.Buttons &= ~X360Button.X;
            }
        }

        public bool y
        {
            get { return  _data.Buttons.HasFlag(X360Button.Y); }
            set {
                if (value)
                    _data.Buttons |= X360Button.Y;
                else
                    _data.Buttons &= ~X360Button.Y;
            }
        }

        public bool lb
        {
            get { return  _data.Buttons.HasFlag(X360Button.LB); }
            set {
                if (value)
                    _data.Buttons |= X360Button.LB;
                else
                    _data.Buttons &= ~X360Button.LB;
            }
        }

        public bool rb
        {
            get { return  _data.Buttons.HasFlag(X360Button.RB); }
            set {
                if (value)
                    _data.Buttons |= X360Button.RB;
                else
                    _data.Buttons &= ~X360Button.RB;
            }
        }

        public bool start
        {
            get { return  _data.Buttons.HasFlag(X360Button.Start); }
            set {
                if (value)
                    _data.Buttons |= X360Button.Start;
                else
                    _data.Buttons &= ~X360Button.Start; 
            }
        }

        public bool back
        {
            get { return  _data.Buttons.HasFlag(X360Button.Back); }
            set {
                if (value)
                    _data.Buttons |= X360Button.Back;
                else
                    _data.Buttons &= ~X360Button.Back;
            }
        }

        public bool up
        {
            get { return  _data.Buttons.HasFlag(X360Button.Up); }
            set {
                if (value)
                    _data.Buttons |= X360Button.Up;
                else
                    _data.Buttons &= ~X360Button.Up;
            }
        }

        public bool down
        {
            get { return  _data.Buttons.HasFlag(X360Button.Down); }
            set {
                if (value)
                    _data.Buttons |= X360Button.Down;
                else
                    _data.Buttons &= ~X360Button.Down;
            }
        }

        public bool left
        {
            get { return  _data.Buttons.HasFlag(X360Button.Left);  }
            set {
                if (value)
                    _data.Buttons |= X360Button.Left;
                else
                    _data.Buttons &= ~X360Button.Left;
            }
        }

        public bool right
        {
            get { return  _data.Buttons.HasFlag(X360Button.Right); }
            set {
                if (value)
                    _data.Buttons |= X360Button.Right;
                else
                    _data.Buttons &= ~X360Button.Right;
            }
        }

        public bool ls
        {
            get { return _data.Buttons.HasFlag(X360Button.LS); }
            set
            {
                if (value)
                    _data.Buttons |= X360Button.LS;
                else
                    _data.Buttons &= ~X360Button.LS;
            }
        }

        public bool rs
        {
            get { return _data.Buttons.HasFlag(X360Button.RS); }
            set
            {
                if (value)
                    _data.Buttons |= X360Button.RS;
                else
                    _data.Buttons &= ~X360Button.RS;
            }
        }

        /// <summary>
        /// Acceptable values range 0 - 1
        /// </summary>
        public double lt
        {
            get { return  _data.LT/255.0; }
            set
            {
                
                //if(value >= 0 && value <=1)
                if (isBetween(value, 0, 1))
                {
                    var v = ensureMapRange(value, 0, 1, 0, 255);
                    _data.LT = (byte) v;
                }
            }
        }

        
        public double rt
        {
            get { return _data.RT/255.0; }
            set
            {
                if(isBetween(value,0,1))
                {
                    var v = ensureMapRange(value, 0, 1, 0, 255);
                    _data.RT = (byte)v;
                }
                //_data.RT = (byte) (value * 255);
            }
        }

        

        

        
        public double lx
        {
            get {
                if (_data.LX < 0)
                    return _data.LX / 32768.0;
                else
                    return _data.LX / 32767.0; 
            }
            set
            {
                //Debug.Assert(isBetween(value, 0, 1), "Value must be between 0 and 1 for lx");
                 _data.LX = (short)ensureMapRange(value, -1, 1, -32768, 32767); //(short) (32767.0 * value);
            }
        }

        public double ly
        {
            
            get
            {
                if (_data.LY < 0)
                    return _data.LY / 32768.0;
                
                return _data.LY / 32767.0;
            }
            set
            {
                _data.LY = (short)ensureMapRange(value, 1, -1, -32768, 32767);
            }



        }

        public double rx
        {
            get
            {
                return mapRange(_data.RX, -32768, 32767, -1, 1);
            }
            set { _data.RX = (short) ensureMapRange(value, -1, 1, -32768, 32767); }

        }


        public double ry
        {
            get
            {
                
                if (_data.RY < 0)
                    return _data.RY / 32768.0;
                else
                    return _data.RY / 32767.0;
            }
            set
            {
                _data.RY = (short)ensureMapRange(value, 1, -1, -32768, 32767);
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

        //private double ensureMapRange(double x, double xMin, double xMax, double yMin, double yMax)
        //{
        //    return Math.Max(Math.Min(((x - xMin) / (xMax - xMin)) * (yMax - yMin) + yMin, yMax), yMin);
        //}


    }
    /*
    public class x360ButtonCollection : ButtonCollectionBase<X360Button>
    {
        
    }

    public abstract class ButtonCollectionBase<T> : IEnumerable<KeyValuePair<T, bool>> where T : struct, IConvertible
    {
        protected readonly Dictionary<T, bool> _buttons;
        
        protected ButtonCollectionBase()
        {
            Type buttonEnum = typeof(T);
            if (!buttonEnum.IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            _buttons = new Dictionary<T, bool>();

            foreach (var item in Enum.GetValues(buttonEnum).Cast<T>())
            {
                _buttons.Add(item, false);
            }

            

        }

        //public T ToFlags() 
        //{
        //    T retval = default(T);

        //    foreach (var item in Enum.GetValues(typeof(T)).Cast<T>())
        //    {
        //        if (_buttons[item])
        //            retval |= item;
        //        else
        //            retval &= ~item;
        //    }

        //    return retval;
        //}

        #region enumerable

        public bool this[T index]
        {
            get { return _buttons[index]; }
            set { _buttons[index] = value; }
        }


        public IEnumerator<KeyValuePair<T, bool>> GetEnumerator()
        {
            return _buttons.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_buttons).GetEnumerator();
        }

        #endregion

    }*/
}
