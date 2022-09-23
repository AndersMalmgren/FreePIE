using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreePIE.Core.Contracts;
//using FreePIE.Core.Plugins.Globals;
using ScpDotNet.Data;


namespace ScpDotNet
{
    [GlobalType(Type = typeof(ScpPluginGlobal), IsIndexed = true)]
    public class ScpPlugin : IPlugin
    {
        private List<ScpPluginGlobal> _globals;

        internal ScpDriver scpDriver;

        /// <summary>
        /// global flips this if an error occured plugging in
        /// </summary>
        internal bool _errorOccured = false;

        public bool IsConnected
        {
            get
            {
                if (scpDriver != null && scpDriver.CurrentState != ConnectionState.Disconnected)
                    return true;

                return false;
            }
        }

        public object CreateGlobal()
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

        public bool GetProperty(int index, IPluginProperty property)
        {
            throw new NotImplementedException();
        }

        public bool SetProperties(Dictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        public void DoBeforeNextExecute()
        {
            _globals.ForEach(d => d.Update());
        }

        public string FriendlyName { get { return "Scp Virtual Bus"; } }

        /*
        public ScpPlugin()
        {
            
        }
        */
        public Action Start()
        {
            return () =>
            {
                scpDriver = new ScpDriver();
                if (scpDriver.Open())
                {

                }
            };
        }

        public void Stop()
        {
            _globals.ForEach(d => d.Update());
            
        }

        public event EventHandler Started;
    }

    [Global(Name = "scp")]
    public class ScpPluginGlobal
    {
        public readonly int playerNum = 0;
        private ScpPlugin _scpPlugin;

        private SCP_XINPUT_DATA _data;
        

        public ScpPluginGlobal(int index, ScpPlugin plugin)
        {
            _scpPlugin = plugin;
            if (plugin._errorOccured)
                return;

            

            int player = (index + 1);

            if (_scpPlugin.scpDriver.Plugin(player))
                this.playerNum = player;
            else
                plugin._errorOccured = true;

            System.Diagnostics.Debug.Assert(this.playerNum > 0, string.Format("Failed to plugin virtual xInput controller {0}", this.playerNum));

        }

        internal void Update()
        {
            if (_scpPlugin.IsConnected)
            {
                _data = new SCP_XINPUT_DATA
                {
                    code1 = 0x1C,
                    ControllerNumber = playerNum,
                    Code2 = 0x14
                };

                byte[] rumble = new byte[2];
                _scpPlugin.scpDriver.Report(_data, rumble);
            }
        }

        internal void Stop()
        {
            if (_scpPlugin.IsConnected)
            {
                bool success =_scpPlugin.scpDriver.Unplug(playerNum);
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

        public bool leftShoulder
        {
            get { return  _data.Buttons.HasFlag(X360Button.LB); }
            set {
                if (value)
                    _data.Buttons |= X360Button.LB;
                else
                    _data.Buttons &= ~X360Button.LB;
            }
        }

        public bool rightShoulder
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

        
        public double leftTrigger
        {
            get { return  _data.LT * 255; }
            set { _data.LT = (byte) (value/255); }
        }

        
        public double rightTrigger
        {
            get { return _data.RT * 255; }
            set { _data.RT |= (byte) (value/255); }
        }

        

        public bool leftThumb
        {
            get { return  _data.Buttons.HasFlag(X360Button.LS); }
        }

        public bool rightThumb
        {
            get { return  _data.Buttons.HasFlag(X360Button.RS); }
        }

        
        public double leftStickX
        {
            get {
                if (_data.LX < 0)
                    return _data.LX / 32768.0;
                else
                    return _data.LX / 32767.0; ;
            }
            set { _data.LX = (short) (32767.0 * value); }
        }

        public double leftStickY
        {
            
            get
            {
                if (_data.LY < 0)
                    return _data.LY / 32768.0;
                else
                    return _data.LY / 32767.0;
            }
            set { _data.LY = (short)(32767.0 * value); }



        }

        public double rightStickX
        {
            get
            {
                if (_data.RY < 0)
                    return _data.RX / 32768.0;
                else
                    return _data.RX / 32767.0;
            }
        }


        public double rightStickY
        {

            get
            {
                if (_data.RY < 0)
                    return _data.RY / 32768.0;
                else
                    return _data.RY / 32767.0;
            }
            set { _data.RY = (short)(32767.0 * value); }



        }

        
        

        
    }
}
